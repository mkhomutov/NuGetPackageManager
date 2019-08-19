namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.Fody;
    using Catel.Logging;
    using Catel.MVVM;
    using NuGet.Configuration;
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

        public PackageDetailsViewModel(IRepositoryService repositoryService, IPackageSearchMetadata packageMetadata)
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

        protected override Task InitializeAsync()
        {
            //by default last version always selected for user actions
            SelectedVersion = Package.LastVersion;

            VersionsCollection = new ObservableCollection<NuGetVersion>() { SelectedVersion };

            _packageMetadataProvider = InitiMetadataProvider();

            return base.InitializeAsync();
        }


        [Model]
        [Expose("Title")]
        [Expose("Description")]
        public NuGetPackage Package { get; set; }

        public ObservableCollection<NuGetVersion> VersionsCollection { get; set; }

        public NuGetVersion SelectedVersion { get; set; }

        public NuGetVersion InstalledVersion { get; set; }

        public Command LoadInfoAboutVersions { get; set; }

        private void LoadInfoAboutVersionsExecute()
        {
            try
            {
                //todo check is initialized?
                if (Package.Versions == null)
                {
                    Package.LoadVersionsAsync().Wait(500);
                    using (var cts = new CancellationTokenSource())
                    {
                        //todo prerelease inclusion
                        _packageMetadataProvider.GetPackageMetadataAsync(Package.Identity, false, cts.Token);
                    }

                    VersionsCollection = new ObservableCollection<NuGetVersion>(Package.Versions);
                }
            }
            catch(TimeoutException ex)
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
    }
}
