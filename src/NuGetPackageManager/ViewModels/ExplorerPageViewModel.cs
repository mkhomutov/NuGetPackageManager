namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.Collections;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Windows.Input;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Models;
    using NuGetPackageManager.Pagination;
    using NuGetPackageManager.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class ExplorerPageViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private static readonly int _pageSize = 17;

        private readonly IPackagesLoaderService _packagesLoaderService;
        private readonly ICommandManager _commandManager;
        private readonly IPackageMetadataMediaDownloadService _packageMetadataMediaDownloadService;

        private ExplorerSettingsContainer _settings;

        private FastObservableCollection<IPackageSearchMetadata> _packages { get; set; }


        public ExplorerPageViewModel(ExplorerSettingsContainer explorerSettings, string pageTitle, IPackagesLoaderService packagesLoaderService,
            IPackageMetadataMediaDownloadService packageMetadataMediaDownloadService, ICommandManager commandManager)
        {
            Title = pageTitle;

            Argument.IsNotNull(() => packagesLoaderService);
            Argument.IsNotNull(() => explorerSettings);
            Argument.IsNotNull(() => packageMetadataMediaDownloadService);
            Argument.IsNotNull(() => commandManager);

            _packagesLoaderService = packagesLoaderService;
            _packageMetadataMediaDownloadService = packageMetadataMediaDownloadService;
            _commandManager = commandManager;

            Settings = explorerSettings;

            LoadNextPackagePage = new TaskCommand(LoadNextPackagePageExecute);
            CancelPageLoading = new TaskCommand(CancelPageLoadingExecute);
            RefreshCurrentPage = new TaskCommand(RefreshCurrentPageExecute);

            commandManager.RegisterCommand(nameof(RefreshCurrentPage), RefreshCurrentPage, this);
        }

        private PageContinuation PageInfo { get; set; }

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
                        //recreate pageinfo
                        PageInfo = new PageContinuation(_pageSize, Settings.ObservedFeed.Source);
                    }

                    //only if page is active
                    //for others update should be delayed until page selected
                    if (IsActive)
                    {
                        await RefreshPageWithNewParameters();
                    }
                }
            }
        }

        public bool IsActive { get; set; }

        public bool IsCancellationTokenAlive { get; set; }

        public CancellationTokenSource PageLoadingTokenSource { get; set; }

        public IPackageSearchMetadata SelectedPackage { get; set; }

        protected async override Task InitializeAsync()
        {
            try
            {
                _packages = new FastObservableCollection<IPackageSearchMetadata>();

                if (Settings.ObservedFeed != null)
                {
                    PageInfo = new PageContinuation(_pageSize, Settings.ObservedFeed.Source);

                    await LoadPackagesForTestAsync(PageInfo);
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
            await LoadPackagesForTestAsync(PageInfo);
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

        private async Task LoadPackagesForTestAsync(PageContinuation pageContinue)
        {
            try
            {
                using (PageLoadingTokenSource = new CancellationTokenSource())
                {
                    IsCancellationTokenAlive = true;

                    var packages = await _packagesLoaderService.LoadAsync(
                        Settings.SearchString, PageInfo, new SearchFilter(Settings.IsPreReleaseIncluded), PageLoadingTokenSource.Token);

                    await DownloadAllPicturesForMetadataAsync(packages);

                    Packages.AddRange(packages);

                    Log.Info($"Page {Title} updates with {packages.Count()} returned by query '{Settings.SearchString}'");
                }
            }
            catch (OperationCanceledException e)
            {
                Log.Info($"Command {nameof(LoadPackagesForTestAsync)} was cancelled by {e}");
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
            PageInfo.Reset();
            Packages.Clear();

            await LoadPackagesForTestAsync(PageInfo);
        }
    }
}
