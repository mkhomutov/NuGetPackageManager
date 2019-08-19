﻿namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.Collections;
    using Catel.Data;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Services;
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
        private static readonly int SingleTasksDelayMs = 800;
        private static readonly IHttpExceptionHandler<FatalProtocolException> packageLoadingExceptionHandler = new FatalProtocolExceptionHandler();

        private readonly IPackagesLoaderService _packagesLoaderService;
        private readonly IRepositoryService _repositoryService;
        private readonly IPackageMetadataMediaDownloadService _packageMetadataMediaDownloadService;
        private readonly INuGetFeedVerificationService _nuGetFeedVerificationService;
        private readonly IDispatcherService _dispatcherService;

        private static readonly System.Timers.Timer SingleDelayTimer = new System.Timers.Timer(SingleTasksDelayMs);

        /// <summary>
        /// Repository context. 
        /// Due to all pages uses package sources selected by user in settings
        /// context is shared between pages too
        /// </summary>
        private static IDisposable Context
        {
            get { return _context; }
            set
            {
                if (_context != value)
                {
                    _context?.Dispose();
                    _context = value;
                }
            }
        }

        private static IDisposable _context;

        private ExplorerSettingsContainer _settings;

        private FastObservableCollection<IPackageSearchMetadata> _packages { get; set; }

        public ExplorerPageViewModel(ExplorerSettingsContainer explorerSettings, string pageTitle, IPackagesLoaderService packagesLoaderService,
            IPackageMetadataMediaDownloadService packageMetadataMediaDownloadService, INuGetFeedVerificationService nuGetFeedVerificationService,
            ICommandManager commandManager, IDispatcherService dispatcherService, IRepositoryService repositoryService)
        {
            Title = pageTitle;

            Argument.IsNotNull(() => packagesLoaderService);
            Argument.IsNotNull(() => explorerSettings);
            Argument.IsNotNull(() => packageMetadataMediaDownloadService);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => nuGetFeedVerificationService);
            Argument.IsNotNull(() => dispatcherService);

            _packagesLoaderService = packagesLoaderService;
            _dispatcherService = dispatcherService;
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

        private PackageSearchParameters AwaitedSearchParameters { get; set; }

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

        public FastObservableCollection<IPackageSearchMetadata> Packages
        {
            get { return _packages; }
            set
            {
                _packages = value;
                RaisePropertyChanged(() => Packages);
            }
        }

        //handle settings changes and force reloading if needed
        private async void OnSettingsPropertyPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Settings.ObservedFeed == null)
            {
                return;
            }

            if (String.Equals(e.PropertyName, nameof(Settings.IsPreReleaseIncluded)) ||
                String.Equals(e.PropertyName, nameof(Settings.SearchString)) || String.Equals(e.PropertyName, nameof(Settings.ObservedFeed)))
            {
                if (IsActive)
                {
                    StartLoadingTimer();
                }
            }
        }

        public bool IsActive { get; set; }

        public bool IsCancellationTokenAlive { get; set; }

        public bool IsLoadingInProcess { get; set; }

        public bool IsCancellationForced { get; set; }

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
                    var searchParams = new PackageSearchParameters(Settings.IsPreReleaseIncluded, Settings.SearchString);
                    await VerifySourceAndLoadPackagesAsync(PageInfo, currentFeed, searchParams);
                }
                else
                {
                    Log.Info("None of the source feeds configured");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private void StartLoadingTimer()
        {
            if (SingleDelayTimer.Enabled)
            {
                SingleDelayTimer.Stop();
            }

            SingleDelayTimer.Start();
        }

        private async void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Log.Info("Timer elapsed");
            var currentFeed = Settings.ObservedFeed;
            //reset page
            PageInfo = new PageContinuation(PageSize, currentFeed.GetPackageSource());

            var searchParams = new PackageSearchParameters(Settings.IsPreReleaseIncluded, Settings.SearchString);
            await VerifySourceAndLoadPackagesAsync(PageInfo, currentFeed, searchParams);
        }

        private async Task VerifySourceAndLoadPackagesAsync(PageContinuation pageinfo, INuGetSource currentSource, PackageSearchParameters searchParams)
        {
            try
            {
                if(pageinfo.Source.IsMultipleSource)
                {
                    Context = _repositoryService.AcquireContext();
                }
                else
                {
                    Context = _repositoryService.AcquireContext((PackageSource)pageinfo.Source);
                }


                if (IsActive)
                {
                    IsCancellationTokenAlive = true;
                    Log.Info("You can now cancel search from gui");
                    using (PageLoadingTokenSource = CreateCanclellationTokenSource())
                    {
                        if (!currentSource.IsVerified)
                        {

                            await CanFeedBeLoadedAsync(VerificationTokenSource.Token, currentSource);
                        }

                        if (!currentSource.IsAccessible)
                        {
                            IsCancellationTokenAlive = false;
                            return;
                        }

                        if (!IsLoadingInProcess)
                        {

                            await LoadPackagesAsync(pageinfo, PageLoadingTokenSource.Token, searchParams);
                        }
                        else
                        {
                            if (IsCancellationForced)
                            {
                                //to prevent load restarting if cancellation initiated by user
                                AwaitedPageInfo = null;
                            }
                            else
                            {
                                AwaitedPageInfo = PageInfo;
                                AwaitedSearchParameters = searchParams;
                            }
                            PageLoadingTokenSource.Cancel();
                        }
                    }
                    IsCancellationTokenAlive = false;
                    PageLoadingTokenSource = null;
                }
            }
            catch (OperationCanceledException e)
            {
                Log.Info($"Command {nameof(LoadPackagesAsync)} was cancelled by {e}");

                IsCancellationTokenAlive = false;

                //backward page if needed
                if (PageInfo.LastNumber > PageSize)
                {
                    PageInfo.GetPrevious();
                }

                //restart
                if (AwaitedPageInfo != null)
                {
                    var awaitedPageinfo = AwaitedPageInfo;
                    var awaitedSeachParams = AwaitedSearchParameters;
                    AwaitedPageInfo = null;
                    AwaitedSearchParameters = null;
                    await VerifySourceAndLoadPackagesAsync(awaitedPageinfo, Settings.ObservedFeed, awaitedSeachParams);
                }
                else
                {
                    Log.Info("Search operation was canceled (interrupted by next user request");
                }
            }
            catch (FatalProtocolException ex)
            {
                IsCancellationTokenAlive = false;
                var result = packageLoadingExceptionHandler.HandleException(ex, currentSource.Source);

                if (result == FeedVerificationResult.AuthenticationRequired)
                {
                    Log.Error($"Authentication credentials required. Cannot load packages from source '{currentSource.Source}'");
                }
                else Log.Error(ex);
            }
            catch (Exception ex)
            {
                IsCancellationTokenAlive = false;
                Log.Error(ex);
            }
        }

        private CancellationTokenSource CreateCanclellationTokenSource()
        {
            if (PageLoadingTokenSource == null)
            {
                return new CancellationTokenSource();
            }

            if (PageLoadingTokenSource.IsCancellationRequested)
            {
                return new CancellationTokenSource();
            }

            return PageLoadingTokenSource;
        }

        #region commands
        public TaskCommand LoadNextPackagePage { get; set; }

        private async Task LoadNextPackagePageExecute()
        {
            var pageInfo = PageInfo;
            var searchParams = new PackageSearchParameters(Settings.IsPreReleaseIncluded, Settings.SearchString);
            await VerifySourceAndLoadPackagesAsync(pageInfo, Settings.ObservedFeed, searchParams);
        }

        public TaskCommand CancelPageLoading { get; set; }

        private async Task CancelPageLoadingExecute()
        {
            IsCancellationForced = true;
            //force cancel all operations
            if (IsCancellationTokenAlive)
            {
                PageLoadingTokenSource.Cancel();
            }

            IsCancellationForced = false;
        }

        public TaskCommand RefreshCurrentPage { get; set; }

        private async Task RefreshCurrentPageExecute()
        {
            if (IsActive)
            {
                StartLoadingTimer();
            }
        }

        #endregion

        private async Task LoadPackagesAsync(PageContinuation pageInfo, CancellationToken cancellationToken, PackageSearchParameters searchParameters)
        {
            try
            {
                IsLoadingInProcess = true;

                if (PageInfo.Current < 0)
                {
                    Packages.Clear();
                }

                var packages = await _packagesLoaderService.LoadAsync(
                    searchParameters.SearchString, pageInfo, new SearchFilter(searchParameters.IsPrereleaseIncluded), cancellationToken);

                await DownloadAllPicturesForMetadataAsync(packages, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                _dispatcherService.BeginInvoke(() =>
                    {
                        Packages.AddRange(packages);
                    }
                );

                Log.Info($"Page {Title} updates with {packages.Count()} returned by query '{Settings.SearchString} from {PageInfo.Source}'");

                IsLoadingInProcess = false;

            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                IsLoadingInProcess = false;
            }
        }

        private async Task DownloadAllPicturesForMetadataAsync(IEnumerable<IPackageSearchMetadata> metadatas, CancellationToken token)
        {
            foreach (var metadata in metadatas)
            {
                if (metadata.IconUrl != null)
                {
                    try
                    {
                        token.ThrowIfCancellationRequested();
                        await _packageMetadataMediaDownloadService.DownloadFromAsync(metadata);
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                }
            }

            await Task.CompletedTask;
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
                        }
                    }

                    unaccessibleFeeds.ForEach(x => combinedSource.RemoveFeed(x));
                }
                else Log.Error($"Parameter {source} has invalid type");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
