using NuGet.Packaging.Core;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface IPackageInstallationService
    {
        Task Install(PackageIdentity identity, IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken);

    }
}
