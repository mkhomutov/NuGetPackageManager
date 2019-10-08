using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using static NuGet.Protocol.Core.Types.PackageSearchMetadataBuilder;

namespace NuGetPackageManager.Packaging
{
    public class UpdatePackageSearchMetadata : ClonedPackageSearchMetadata
    {
        public VersionInfo FromVersion { get; set; }
    }
}
