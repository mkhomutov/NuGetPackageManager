using NuGet.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management
{
    public interface INuGetExtensibleProjectManager
    {
        bool IsPackageInstalled(IExtensibleProject project, NuGet.Packaging.Core.PackageIdentity package);
    }
}
