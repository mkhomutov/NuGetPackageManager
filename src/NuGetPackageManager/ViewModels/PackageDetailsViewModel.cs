using Catel.Fody;
using Catel.MVVM;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Models;

namespace NuGetPackageManager.ViewModels
{
    public class PackageDetailsViewModel : ViewModelBase
    {
        public PackageDetailsViewModel(IPackageSearchMetadata packageMetadata)
        {
            //create package from metadata
            if (packageMetadata != null)
            {
                Package = new NuGetPackage(packageMetadata);
            }
        }

        [Model]
        [Expose("Title")]
        [Expose("Description")]
        public NuGetPackage Package { get; set; }
    }
}
