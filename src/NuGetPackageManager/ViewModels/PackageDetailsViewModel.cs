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
    using NuGetPackageManager.Interfaces;
    using NuGetPackageManager.Management;
    using NuGetPackageManager.Models;
    using NuGetPackageManager.Providers;
    using NuGetPackageManager.Services;
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

        public PackageDetailsViewModel(IPackageSearchMetadata packageMetadata, IRepositoryService repositoryService)
        {
            Argument.IsNotNull(() => repositoryService);
            _repositoryService = repositoryService;

            //create package from metadata
            if (packageMetadata != null)
            {
                Package = new NuGetPackage(packageMetadata);
            }

            LoadInfoAboutVersions = new Command(LoadInfoAboutVersionsExecute, () => Package != null);
        }

        protected async override Task InitializeAsync()
        {
            try
            {
                //by default last version always selected for user actions
                SelectedVersion = Package.LastVersion;

                VersionsCollection = new ObservableCollection<NuGetVersion>() { SelectedVersion };

                _packageMetadataProvider = InitiMetadataProvider();

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
                    identity, ExplorerSettingsContainer.Singleton.IsPreReleaseIncluded, cts.Token);

                DependencyInfo = VersionData.DependencySets;
            }
        }

        [Model]
        [Expose("Title")]
        [Expose("Description")]
        public NuGetPackage Package { get; set; }

        public ObservableCollection<NuGetVersion> VersionsCollection { get; set; }

        public object DependencyInfo { get; set; }

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

        private IPackageMetadataProvider InitiMetadataProvider()
        {
            var currentSourceContext = SourceContext.CurrentContext;

            var repositories = currentSourceContext.Sources ?? currentSourceContext.PackageSources.Select(src => _repositoryService.GetRepository(src));

            return new PackageMetadataProvider(repositories, null);
        }

        protected override async void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (string.Equals(e.PropertyName, nameof(SelectedVersion)))
            {
                if (SelectedVersion != Package.Identity.Version)
                {
                    var identity = new PackageIdentity(Package.Identity.Id, SelectedVersion);
                    await LoadSinglePackageMetadataAsync(identity);
                }
            }
        }
    }
}
