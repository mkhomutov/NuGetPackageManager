using Catel.MVVM;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Enums;
using NuGetPackageManager.Models;
using NuGetPackageManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Pagination
{
    public class DeferToken
    {
        public Func<IPackageSearchMetadata> PackageSelector { get; set; }

        public PageType LoadType { get; set; }

        public NuGetPackage Package { get; set; }

        public Action<PackageStatus> UpdateAction { get; set; }
    }
}
