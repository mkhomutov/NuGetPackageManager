using Catel;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Enums;
using NuGetPackageManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Packaging
{
    public class NuGetPackageCombinator
    {
        /// <summary>
        /// Combines NuGet Package with other metadata
        /// and produce state from their relation
        /// </summary>
        public static async Task<PackageStatus> Combine(NuGetPackage package, PageType tokenPage, IPackageSearchMetadata metadata)
        {
            Argument.IsNotNull(() => metadata);
            Argument.IsNotNull(() => package);

            if (tokenPage == PageType.Browse)
            {
                await package.MergeMetadata(metadata);

                //keep installed version same, because this NuGetPackage
                //created from local installed nupkg metadata.
            }

            if(tokenPage == PageType.Installed)
            {
                //then original package retrived from real source and should be merged with
                //installed local metadata

                await package.MergeMetadata(metadata);

                package.InstalledVersion = metadata.Identity.Version;
            }

            if(tokenPage == PageType.Updates)
            {
                return PackageStatus.NotInstalled;
            }

            var comparison = package.InstalledVersion.CompareTo(package.LastVersion, NuGet.Versioning.VersionComparison.VersionRelease);

            return (PackageStatus)comparison;
        }
    }
}
