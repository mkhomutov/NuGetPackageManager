namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.Data;
    using Catel.Fody;
    using Catel.Logging;
    using Catel.MVVM;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;
    using NuGetPackageManager.Enums;
    using NuGetPackageManager.Management;
    using NuGetPackageManager.Models;
    using NuGetPackageManager.Pagination;
    using NuGetPackageManager.Providers;
    using NuGetPackageManager.Services;
    using NuGetPackageManager.Windows;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PackageDetailsViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private IPackageMetadataProvider _packageMetadataProvider;

        private IRepositoryService _repositoryService;

        private IPackageInstallationService _installationService;

        private IModelProvider<ExplorerSettingsContainer> _settingsProvider;

        private IProgressManager _progressManager;

        private INuGetExtensibleProjectManager _projectManager;
        

        public PackageDetailsViewModel(IPackageSearchMetadata packageMetadata, MetadataOrigin fromPage, IRepositoryService repositoryService, IModelProvider<ExplorerSettingsContainer> settingsProvider,
            IPackageInstallationService installationService, IProgressManager progressManager, INuGetExtensibleProjectManager projectManager)
        {
            Argument.IsNotNull(() => repositoryService);
            Argument.IsNotNull(() => settingsProvider);
            Argument.IsNotNull(() => installationService);
            Argument.IsNotNull(() => progressManager);
            Argument.IsNotNull(() => projectManager);

            _repositoryService = repositoryService;
            _settingsProvider = settingsProvider;
            _installationService = installationService;
            _progressManager = progressManager;
            _projectManager = projectManager;

            //create package from metadata
            if (packageMetadata != null)
            {
                Package = new NuGetPackage(packageMetadata);
            }

            if (fromPage == MetadataOrigin.Browse)
            {
                //installed version is unknown until installed is loaded
                Package.InstalledVersion = null;
            }
            if (fromPage == MetadataOrigin.Installed)
            {
                Package.InstalledVersion = Package.LastVersion;
            }

            IsDownloadCountShowed = fromPage != MetadataOrigin.Installed;

            LoadInfoAboutVersions = new Command(LoadInfoAboutVersionsExecute, () => Package != null);
            InstallPackage = new TaskCommand(OnInstallPackageExecute, () => NuGetActionTarget?.IsValid ?? false);
            UninstallPackage = new TaskCommand(OnUninstallPackageExecute, () => NuGetActionTarget?.IsValid ?? false);
        }

        private bool OnInstallPackageCanExecute()
        {
            return NuGetActionTarget?.IsValid ?? false;
        }

        protected async override Task InitializeAsync()
        {
            try
            {
                //select identity version
                SelectedVersion = Package.Identity.Version;

                VersionsCollection = new ObservableCollection<NuGetVersion>() { SelectedVersion };

                NuGetActionTarget.PropertyChanged += OnNuGetActionTargetPropertyPropertyChanged;                   

                _packageMetadataProvider = InitMetadataProvider();

                await LoadSinglePackageMetadataAsync(Package.Identity);
            }
            catch (Exception e)
            {
                Log.Error(e, "Error ocurred during view model inititalization, probably package metadata is incorrect");
            }
        }

        protected override Task CloseAsync()
        {
            NuGetActionTarget.PropertyChanged -= OnNuGetActionTargetPropertyPropertyChanged;

            return base.CloseAsync();
        }

        protected async Task LoadSinglePackageMetadataAsync(PackageIdentity identity)
        {
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    //todo include prerelease
                    VersionData = await _packageMetadataProvider?.GetPackageMetadataAsync(
                        identity, _settingsProvider.Model.IsPreReleaseIncluded, cts.Token);

                    DependencyInfo = VersionData?.DependencySets;
                }
            }
            catch(Exception e)
            {
                Log.Error(e, "Metadata retrieve error");
            }
        }


        [Model]
        [Expose("Title")]
        [Expose("Description")]
        [Expose("Summary")]
        [Expose("DownloadCount")]
        [Expose("Authors")]
        [Expose("IconUrl")]
        [Expose("Identity")]
        [Expose("Status")]
        public NuGetPackage Package { get; set; }

        public ObservableCollection<NuGetVersion> VersionsCollection { get; set; }

        public object DependencyInfo { get; set; }

        public DeferToken DefferedLoadingToken { get; set; }

        [ViewModelToModel]
        public PackageStatus Status { get; set; }

        public bool IsDownloadCountShowed { get; }

        public NuGetActionTarget NuGetActionTarget { get; } = new NuGetActionTarget();

        public IPackageSearchMetadata VersionData { get; set; }

        public NuGetVersion SelectedVersion { get; set; }

        public PackageIdentity SelectedPackage => new PackageIdentity(Package.Identity.Id, SelectedVersion);

        [ViewModelToModel]
        public NuGetVersion InstalledVersion { get; set; }

        public int SelectedVersionIndex { get; set; }

        public Command LoadInfoAboutVersions { get; set; }

        private void LoadInfoAboutVersionsExecute()
        {
            try
            {
                if (Package.LoadVersionsAsync().Wait(500))
                {
                    VersionsCollection = new ObservableCollection<NuGetVersion>(Package.Versions);
                }
                else
                {
                    throw new TimeoutException();
                }
            }
            catch (TimeoutException ex)
            {
                Log.Error(ex, "Failed to get package versions for a given time (500 ms)");
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        public TaskCommand InstallPackage { get; set; }

        private async Task OnInstallPackageExecute()
        {
            try
            {
                _progressManager.ShowBar(this);

                using(var cts = new CancellationTokenSource())
                {
                    await _projectManager.InstallPackageForMultipleProject(NuGetActionTarget.TargetProjects, SelectedPackage, cts.Token);
                }

                await Task.Delay(200);

                _progressManager.HideBar(this);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error when installing package {Package.Identity}, installation was failed");
            }
        }

        public TaskCommand UninstallPackage { get; set; }

        private async Task OnUninstallPackageExecute()
        {
            try
            {
                _progressManager.ShowBar(this);

                using (var cts = new CancellationTokenSource())
                {
                    await _projectManager.UninstallPackageForMultipleProject(NuGetActionTarget.TargetProjects, SelectedPackage, cts.Token);
                }

                await Task.Delay(200);

                _progressManager.HideBar(this);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Error when uninstalling package {Package.Identity}, uninstall was failed");
            }
        }

        private IPackageMetadataProvider InitMetadataProvider()
        {
            var currentSourceContext = SourceContext.CurrentContext;

            var repositories = currentSourceContext.Repositories ?? currentSourceContext.PackageSources.Select(src => _repositoryService.GetRepository(src));

            return new PackageMetadataProvider(repositories, null);
        }

        private void OnNuGetActionTargetPropertyPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var commandManager = this.GetViewModelCommandManager();
            commandManager.InvalidateCommands();
        }

        protected override async void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (string.Equals(e.PropertyName, nameof(SelectedVersion)))
            {
                if (e.OldValue == null && SelectedVersion == Package.Identity.Version)
                {
                    //skip loading on version list first load
                    return;
                }

                var identity = new PackageIdentity(Package.Identity.Id, SelectedVersion);
                await LoadSinglePackageMetadataAsync(identity);
            }
        }
    }
}
