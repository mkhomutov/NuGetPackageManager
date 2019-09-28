using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Enums;
using NuGetPackageManager.Models;
using System;

namespace NuGetPackageManager.Pagination
{
    public class DeferToken
    {
        public Func<IPackageSearchMetadata> PackageSelector { get; set; }

        public MetadataOrigin LoadType { get; set; }

        public NuGetPackage Package { get; set; }

        public Action<PackageStatus> UpdateAction { get; set; }
    }
}
