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
    using NuGetPackageManager.Management;
    using NuGetPackageManager.Models;
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

        public PackageDetailsViewModel(IPackageSearchMetadata packageMetadata, IRepositoryService repositoryService, IModelProvider<ExplorerSettingsContainer> settingsProvider,
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

            LoadInfoAboutVersions = new Command(LoadInfoAboutVersionsExecute, () => Package != null);
            InstallPackage = new TaskCommand(InstallPackageExecute, () => NuGetActionTarget?.IsValid ?? false);
            UninstallPackage = new TaskCommand(UninstallPackageExecute, () => NuGetActionTarget?.IsValid ?? false);
        }

        protected async override Task InitializeAsync()
        {
            try
            {
                //by default last version always selected for user actions
                SelectedVersion = Package.LastVersion;

                VersionsCollection = new ObservableCollection<NuGetVersion>() { SelectedVersion };

                _packageMetadataProvider = InitMetadataProvider();

                await LoadSinglePackageMetadataAsync();
            }
            catch (Exception e)
            {
                Log.Error(e, "Error ocurred during view model inititalization, probably package metadata is incorrect");
            }
        }

        protected async Task LoadSinglePackageMetadataAsync()
        {
            await LoadSinglePackageMetadataAsync(Package.Identity);
        }

        protected async Task LoadSinglePackageMetadataAsync(PackageIdentity identity)
        {
            using (var cts = new CancellationTokenSource())
            {
                //todo include prerelease
                VersionData = await _packageMetadataProvider?.GetPackageMetadataAsync(
                    identity, _settingsProvider.Model.IsPreReleaseIncluded, cts.Token);

                DependencyInfo = VersionData.DependencySets;
            }
        }

        [Model]
        [Expose("Title")]
        [Expose("Description")]
        public NuGetPackage Package { get; set; }

        public ObservableCollection<NuGetVersion> VersionsCollection { get; set; }

        public object DependencyInfo { get; set; }

        public NuGetActionTarget NuGetActionTarget { get; } = new NuGetActionTarget();

        public IPackageSearchMetadata VersionData { get; set; }

        public NuGetVersion SelectedVersion { get; set; }

        public NuGetVersion InstalledVersion { get; set; }

        public int SelectedVersionIndex { get; set; }

        public Command LoadInfoAboutVersions { get; set; }

        private void LoadInfoAboutVersionsExecute()
        {
            try
            {
                //todo check is initialized?
                if (Package.Versions == null)
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

        private async Task InstallPackageExecute()
        {
            try
            {
                _progressManager.ShowBar(this);

                var identity = new PackageIdentity(Package.Identity.Id, SelectedVersion);

                foreach (var project in NuGetActionTarget.TargetProjects)
                {

                    using (var cts = new CancellationTokenSource())
                    {
                        await _projectManager.InstallPackageForProject(project, identity, cts.Token);
                    }
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

        private async Task UninstallPackageExecute()
        {
            try
            {
                var identity = new PackageIdentity(Package.Identity.Id, SelectedVersion);

                foreach (var project in NuGetActionTarget.TargetProjects)
                {
                    using (var cts = new CancellationTokenSource())
                    {
                        var isInstalled = await _projectManager.IsPackageInstalledAsync(project, identity, cts.Token);

                        if (!isInstalled)
                        {
                            continue;
                        }

                        //todo use pm implementation
                        await _installationService.UninstallAsync(identity, project, cts.Token);
                    }
                }
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
