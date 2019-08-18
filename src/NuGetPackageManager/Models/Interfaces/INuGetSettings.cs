using NuGet.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public interface INuGetSettings
    {
        IReadOnlyList<PackageSource> GetAllPackageSources();
    }
}
