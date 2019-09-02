using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface IPackageInstallationService
    {
        Task Install(PackageIdentity identity, IExtensibleProject project, CancellationToken cancellationToken);
    }
}
