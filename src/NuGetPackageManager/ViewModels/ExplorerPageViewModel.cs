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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ExplorerPageViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private static readonly int _pageSize = 17;
        private static readonly int singleTasksDelayMs = 1000;

        private readonly IPackagesLoaderService _packagesLoaderService;
        private readonly ICommandManager _commandManager;
        private readonly IPackageMetadataMediaDownloadService _packageMetadataMediaDownloadService;
        private readonly INuGetFeedVerificationService _nuGetFeedVerificationService;

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
            _commandManager = commandManager;
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
        private async void OnSettingsPropertyPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(Settings.IsPreReleaseIncluded))
                || e.PropertyName.Equals(nameof(Settings.SearchString)) || e.PropertyName.Equals(nameof(Settings.ObservedFeed)))
            {
                if (Settings.ObservedFeed != null)
                {
                    if (e.PropertyName == nameof(Settings.ObservedFeed))
                    {
                        if (IsActive)
                        {
                            if (SingleDelayTimer.Enabled)
                            {
                                SingleDelayTimer.Stop();

                                Log.Info($"Restart timer, {e.PropertyName} property changed");

                                SingleDelayTimer.Start();

                                return;
                            }

                            SingleDelayTimer.Start();
                        }
                    }
                }
            }
        }

        public bool IsActive { get; set; }

        public bool IsCancellationTokenAlive { get; set; }

        public static System.Timers.Timer SingleDelayTimer { get; set; } = new System.Timers.Timer(singleTasksDelayMs);
        public static CancellationTokenSource DelayCancellationTokenSource { get; set; } = new CancellationTokenSource();

        public CancellationTokenSource PageLoadingTokenSource { get; set; }

        public IPackageSearchMetadata SelectedPackage { get; set; }

        protected async override Task InitializeAsync()
        {
            try
            {
                //execution delay
                SingleDelayTimer.Elapsed += SingleDelayTimer_Elapsed;
                SingleDelayTimer.AutoReset = false;

                _packages = new FastObservableCollection<IPackageSearchMetadata>();

                //todo validation
                if (Settings.ObservedFeed != null && !String.IsNullOrEmpty(Settings.ObservedFeed.Source))
                {
                    if (!Settings.ObservedFeed.IsVerified)
                    {
                        using (var cts = new CancellationTokenSource())
                        {
                            //await CheckFeedCanBeLoadedAsync(cts.Token);
                        }
                    }
                    PageInfo = new PageContinuation(_pageSize, Settings.ObservedFeed.GetPackageSource());

                    if (IsActive && PageInfo.IsValid)
                    {
                       // await LoadPackagesAsync();
                    }
                }
                else
                {
                    Log.Info("None of the source feeds configured");
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        private async void SingleDelayTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            var currentFeed = Settings.ObservedFeed;
            PageInfo = new PageContinuation(_pageSize, currentFeed.GetPackageSource());
            await VerifySourceAndLoadPackagesAsync(PageInfo, currentFeed);
        }

        private async Task VerifySourceAndLoadPackagesAsync(PageContinuation pageinfo, INuGetSource currentSource)
        {
            try
            {
                if (IsActive)
                {
                    if (!currentSource.IsVerified)
                    {
                        try
                        {
                            await CheckFeedCanBeLoadedAsync(VerificationTokenSource.Token, currentSource);
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
            catch (Exception)
            {
                throw;
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

        private async Task CheckFeedCanBeLoadedAsync(CancellationToken cancelToken, INuGetSource source)
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
            catch(Exception ex)
            {
                Log.Error(ex);
                throw;
            }
        }
    }
}
