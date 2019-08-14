namespace NuGetPackageManager.ViewModels
{
    using Catel.Fody;
    using Catel.Logging;
    using Catel.MVVM;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;
    using NuGetPackageManager.Models;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public class PackageDetailsViewModel : ViewModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public PackageDetailsViewModel(IPackageSearchMetadata packageMetadata)
        {
            //create package from metadata
            if (packageMetadata != null)
            {
                Package = new NuGetPackage(packageMetadata);
            }

            var deps = packageMetadata.DependencySets;

            LoadInfoAboutVersions = new Command(LoadInfoAboutVersionsExecute, () => Package != null);
        }

        protected override Task InitializeAsync()
        {
            //by default last version always selected for user actions
            SelectedVersion = Package.LastVersion;

            VersionsCollection = new ObservableCollection<NuGetVersion>() { SelectedVersion };

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
                    VersionsCollection = new ObservableCollection<NuGetVersion>(Package.Versions);
                }
            }
            catch(TimeoutException ex)
            {
                Log.Error(ex, "failed to get package versions for a given time");
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
