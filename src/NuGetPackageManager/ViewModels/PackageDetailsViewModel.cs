namespace NuGetPackageManager.ViewModels
{
    using Catel.Fody;
    using Catel.MVVM;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;
    using NuGetPackageManager.Models;
    using System;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public class PackageDetailsViewModel : ViewModelBase
    {
        public PackageDetailsViewModel(IPackageSearchMetadata packageMetadata)
        {
            //create package from metadata
            if (packageMetadata != null)
            {
                Package = new NuGetPackage(packageMetadata);
            }

            LoadInfoAboutVersions = new TaskCommand(LoadInfoAboutVersionsExecute, () => Package != null);
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

        public TaskCommand LoadInfoAboutVersions { get; set; }

        private async Task LoadInfoAboutVersionsExecute()
        {
            try
            {
                //todo check is initialized?
                if (Package.Versions == null)
                {
                    await Package.LoadVersionsAsync();
                    VersionsCollection = new ObservableCollection<NuGetVersion>(Package.Versions);
                }
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
