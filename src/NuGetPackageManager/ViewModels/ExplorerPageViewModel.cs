namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.Collections;
    using Catel.Logging;
    using Catel.MVVM;
    using NuGet.Configuration;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Models;
    using NuGetPackageManager.Pagination;
    using NuGetPackageManager.Services;
    using NuGetPackageManager.Web;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ExplorerPageViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private static readonly int PageSize = 17;
        private static readonly int SingleTasksDelayMs = 1000;
        private static readonly IHttpExceptionHandler<FatalProtocolException> packageLoadingExceptionHandler = new FatalProtocolExceptionHandler();

        private readonly IPackagesLoaderService _packagesLoaderService;
        private readonly IPackageMetadataMediaDownloadService _packageMetadataMediaDownloadService;
        private readonly INuGetFeedVerificationService _nuGetFeedVerificationService;

        private static readonly System.Timers.Timer SingleDelayTimer = new System.Timers.Timer(SingleTasksDelayMs);

        private ExplorerSettingsContainer _settings;

        private FastObservableCollection<IPackageSearchMetadata> _packages { get; set; }

        public ExplorerPageViewModel(ExplorerSettingsContainer explorerSettings, string pageTitle, IPackagesLoaderService packagesLoaderService,
            IPackageMetadataMediaDownloadService packageMetadataMediaDownloadService, INuGetFeedVerificationService nuGetFeedVerificationService,
            ICommandManager commandManager)
        {
            Title = pageTitle;

            Argument.IsNotNull(() => packagesLoaderService);
            Argument.IsNotNull(() => explorerSettings);
            Argument.IsNotNull(() => packageMetadataMediaDownloadService);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => nuGetFeedVerificationService);

            _packagesLoaderService = packagesLoaderService;
            _packageMetadataMediaDownloadService = packageMetadataMediaDownloadService;
            _nuGetFeedVerificationService = nuGetFeedVerificationService;

            Settings = explorerSettings;

            LoadNextPackagePage = new TaskCommand(LoadNextPackagePageExecute);
            CancelPageLoading = new TaskCommand(CancelPageLoadingExecute);
            RefreshCurrentPage = new TaskCommand(RefreshCurrentPageExecute);

            commandManager.RegisterCommand(nameof(RefreshCurrentPage), RefreshCurrentPage, this);
        }

        public static CancellationTokenSource VerificationTokenSource { get; set; } = new CancellationTokenSource();

        private PageContinuation PageInfo { get; set; }

        private PageContinuation AwaitedPageInfo { get; set; }

        public ExplorerSettingsContainer Settings
        {
            get { return _settings; }
            set
            {
                if (_settings != null)
                {
                    _settings.PropertyChanged -= OnSettingsPropertyPropertyChanged;
                }
                _settings = value;

                if (_settings != null)
                {
                    _settings.PropertyChanged += OnSettingsPropertyPropertyChanged;
                }
            }
        }

        //handle settings changes and force reloading if needed
        private async void OnSettingsPropertyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(Settings.ObservedFeed == null)
            {
                return;
            }

            if (String.Equals(e.PropertyName, nameof(Settings.ObservedFeed)))
            {
                if (IsActive)
                {
                    if (SingleDelayTimer.Enabled)
                    {
                        SingleDelayTimer.Stop();

                        Log.Info($"Restart timer, {e.PropertyName} property changed");

                    }

                    SingleDelayTimer.Start();
                }
            }

            if (String.Equals(e.PropertyName, nameof(Settings.IsPreReleaseIncluded)) ||
                String.Equals(e.PropertyName, nameof(Settings.SearchString)) ||  String.Equals(e.PropertyName, nameof(Settings.ObservedFeed)))
            {
               //todo
            }
        }

        public bool IsActive { get; set; }

        public bool IsCancellationTokenAlive { get; set; }

        public static CancellationTokenSource DelayCancellationTokenSource { get; set; } = new CancellationTokenSource();

        public CancellationTokenSource PageLoadingTokenSource { get; set; }

        public IPackageSearchMetadata SelectedPackage { get; set; }

        protected async override Task InitializeAsync()
        {
            try
            {
                //execution delay
                SingleDelayTimer.Elapsed += OnTimerElapsed;
                SingleDelayTimer.AutoReset = false;

                _packages = new FastObservableCollection<IPackageSearchMetadata>();

                //todo validation
                if (Settings.ObservedFeed != null && !String.IsNullOrEmpty(Settings.ObservedFeed.Source))
                {
                    var currentFeed = Settings.ObservedFeed;
                    PageInfo = new PageContinuation(PageSize, Settings.ObservedFeed.GetPackageSource());
                    await VerifySourceAndLoadPackagesAsync(PageInfo, currentFeed);
                }
                else
                {
                    Log.Info("None of the source feeds configured");
                }
            }
            catch(Exception ex)
            {
                Log.Error(ex);
            }
        }

        private async void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var currentFeed = Settings.ObservedFeed;
            PageInfo = new PageContinuation(PageSize, currentFeed.GetPackageSource());
            await VerifySourceAndLoadPackagesAsync(PageInfo, currentFeed);
        }

        private async Task VerifySourceAndLoadPackagesAsync(PageContinuation pageinfo, INuGetSource currentSource)
        {
            try
            {
                //todo check is active, interrupt if active changed?
                if (IsActive)
                {
                    if (!currentSource.IsVerified)
                    {
                        try
                        {
                            await CanFeedBeLoadedAsync(VerificationTokenSource.Token, currentSource);
                        }
                        catch (OperationCanceledException)
                        {
                            return;
                        }
                    }

                    if(!Settings.ObservedFeed.IsAccessible)
                    {
                        return;
                    }

                    if (!IsCancellationTokenAlive)
                    {

                        await LoadPackagesAsync();
                    }
                    else
                    {
                        AwaitedPageInfo = PageInfo;
                        PageLoadingTokenSource.Cancel();
                    }
                }
            }      
            catch (FatalProtocolException ex) 
            {
                var result = packageLoadingExceptionHandler.HandleException(ex, currentSource.Source);

                if(result == FeedVerificationResult.AuthenticationRequired)
                {
                    Log.Error($"Authentication credentials required. Cannot load packages from source '{currentSource.Source}'");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public FastObservableCollection<IPackageSearchMetadata> Packages
        {
            get { return _packages; }
            set
            {
                _packages = value;
                RaisePropertyChanged(() => Packages);
            }
        }

        public TaskCommand LoadNextPackagePage { get; set; }
        
        private async Task LoadNextPackagePageExecute()
        {
            await LoadPackagesAsync();
        }

        public TaskCommand CancelPageLoading { get; set; }

        private async Task CancelPageLoadingExecute()
        {
            if (IsCancellationTokenAlive)
            {
                PageLoadingTokenSource.Cancel();
            }
        }

        public TaskCommand RefreshCurrentPage { get; set; }

        private async Task RefreshCurrentPageExecute()
        {
            if(IsActive)
            {
                await RefreshPageWithNewParameters();
            }
        }

        private async Task LoadPackagesAsync()
        {
            try
            {
                IsCancellationTokenAlive = true;

                if(PageInfo.Current < 0)
                {
                    Packages.Clear();
                }

                using (PageLoadingTokenSource = new CancellationTokenSource())
                {

                    var packages = await _packagesLoaderService.LoadAsync(
                        Settings.SearchString, PageInfo, new SearchFilter(Settings.IsPreReleaseIncluded), PageLoadingTokenSource.Token);

                    //await DownloadAllPicturesForMetadataAsync(packages);
                    await Task.Delay(8000, PageLoadingTokenSource.Token);

                    PageLoadingTokenSource.Token.ThrowIfCancellationRequested();

                    Packages.AddRange(packages);

                    Log.Info($"Page {Title} updates with {packages.Count()} returned by query '{Settings.SearchString} from {PageInfo.Source}'");

                    IsCancellationTokenAlive = false;
                }

            }
            catch (OperationCanceledException e)
            {
                Log.Info($"Command {nameof(LoadPackagesAsync)} was cancelled by {e}");

                IsCancellationTokenAlive = false;

                //restart
                if (AwaitedPageInfo != null)
                {
                    var pageinfo = AwaitedPageInfo;
                    AwaitedPageInfo = null;
                    await VerifySourceAndLoadPackagesAsync(pageinfo, Settings.ObservedFeed);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                IsCancellationTokenAlive = false;
            }
        }

        private async Task DownloadAllPicturesForMetadataAsync(IEnumerable<IPackageSearchMetadata> metadatas)
        {
            foreach (var metadata in metadatas)
            {
                if (metadata.IconUrl != null)
                {
                    try
                    {
                        await _packageMetadataMediaDownloadService.DownloadFromAsync(metadata);
                    }
                    catch(Exception)
                    {
                        throw;
                    }
                }
            }

            await Task.CompletedTask;
        }

        private async Task RefreshPageWithNewParameters()
        {
        }

        private async Task CanFeedBeLoadedAsync(CancellationToken cancelToken, INuGetSource source)
        {
            try
            {
                Log.Info($"{source} is verified");

                if (source is NuGetFeed)
                {
                    var singleSource = source as NuGetFeed;

                    singleSource.VerificationResult = singleSource.IsLocal() ? FeedVerificationResult.Valid
                        : await _nuGetFeedVerificationService.VerifyFeedAsync(source.Source, cancelToken);
                }
                else if (source is CombinedNuGetSource)
                {
                    var combinedSource = source as CombinedNuGetSource;
                    var unaccessibleFeeds = new List<NuGetFeed>();

                    foreach (var feed in combinedSource.GetAllSources())
                    {
                        feed.VerificationResult = feed.IsLocal() ? FeedVerificationResult.Valid
                        : await _nuGetFeedVerificationService.VerifyFeedAsync(feed.Source, cancelToken);

                        if (!feed.IsAccessible)
                        {
                            unaccessibleFeeds.Add(feed);
                            Log.Warning($"{feed} is unaccessible. It won't be used when 'All' option selected");
                            //todo isAccessible be true for feed with credentials
                        }
                    }

                    unaccessibleFeeds.ForEach(x => combinedSource.RemoveFeed(x));
                }
                else Log.Error($"Parameter {source} has invalid type");
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
