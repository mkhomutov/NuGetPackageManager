using Catel.Data;
using NuGet.Protocol.Core.Types;

namespace NuGetPackageManager.Models
{
    public class NuGetPackage : ModelBase
    {
        public NuGetPackage(IPackageSearchMetadata packageMetadata)
        {
            Title = packageMetadata.Title;
            Description = packageMetadata.Description;

        }

        public string Title { get; private set; }

        public string Description { get; private set; }
    }
}
