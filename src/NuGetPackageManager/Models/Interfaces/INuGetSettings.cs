using NuGet.Configuration;
using System.Collections.Generic;

namespace NuGetPackageManager.Models
{
    public interface INuGetSettings
    {
        IReadOnlyList<PackageSource> GetAllPackageSources();
    }
}
