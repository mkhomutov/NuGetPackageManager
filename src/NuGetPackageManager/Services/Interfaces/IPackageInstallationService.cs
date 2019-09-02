using NuGet.Packaging.Core;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface IPackageInstallationService
    {
        Task Install(PackageIdentity identity, IExtensibleProject project, CancellationToken cancellationToken);
    }
}
