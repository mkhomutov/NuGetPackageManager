namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.Collections;
    using Catel.Logging;
    using Catel.MVVM;
    using NuGet.Configuration;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Models;
    using NuGetPackageManager.Pagination;
    using NuGetPackageManager.Services;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class ExplorerPageViewModel : ViewModelBase
    {
        private readonly IPackagesLoaderService _packagesLoaderService;
        private ExplorerSettingsContainer _settings;
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private FastObservableCollection<IPackageSearchMetadata> _packages { get; set; }

        public ExplorerPageViewModel(ExplorerSettingsContainer explorerSettings, string pageTitle, IPackagesLoaderService packagesLoaderService)
        {
            Title = pageTitle;

            Argument.IsNotNull(() => packagesLoaderService);
            Argument.IsNotNull(() => explorerSettings);

            _packagesLoaderService = packagesLoaderService;

            Settings = explorerSettings;

            LoadNextPackagePage = new TaskCommand(LoadNextPackagePageExecute);
        }

        private PageContinuation PageInfo { get; set; }

        public ExplorerSettingsContainer Settings
        {
            get { return _settings; }
            set
            {
                if(_settings != null)
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
            if(e.PropertyName == nameof(Settings.IsPreReleaseIncluded) || e.PropertyName == nameof(Settings.SearchString))
            {
                //only if page is active
                //for others update should be delayed until page selected
                if (IsActive)
                {
                    await ResetLoaded();
                }
            }
        }

        public bool IsActive { get; set; }

        protected async override Task InitializeAsync()
        {
            _packages = new FastObservableCollection<IPackageSearchMetadata>();

            PageInfo = new PageContinuation(17, "https://api.nuget.org/v3/index.json");

            await LoadPackagesForTestAsync(PageInfo);
        }

        /// <summary>
        /// Example set of items
        /// </summary>
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

        private async Task LoadPackagesForTestAsync(PageContinuation pageContinue)
        {

            using (var cts = new CancellationTokenSource())
            {
                var rs = new SearchFilter(Settings.IsPreReleaseIncluded);
                var packages = await _packagesLoaderService.LoadAsync(Settings.SearchString, PageInfo, new SearchFilter(Settings.IsPreReleaseIncluded), cts.Token);

                Packages.AddRange(packages);

                Log.Info($"Page {Title} updates with {packages.Count()} returned by query '{Settings.SearchString}'");
            }
        }

        private async Task ResetLoaded()
        {
            PageInfo.Reset();
            Packages.Clear();

            await LoadPackagesForTestAsync(PageInfo);
        }
    }
}
