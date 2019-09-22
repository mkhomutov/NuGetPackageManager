using NuGet.Packaging;
using NuGet.Packaging.Core;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management
{
    public interface INuGetExtensibleProjectManager
    {
        Task InstallPackageForProject(IExtensibleProject project, PackageIdentity package, System.Threading.CancellationToken token);
        Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(IExtensibleProject project, System.Threading.CancellationToken token);
        Task<bool> IsPackageInstalledAsync(IExtensibleProject project, PackageIdentity package, System.Threading.CancellationToken token);
        Task<Packaging.PackageCollection> CreatePackagesCollectionFromProjectsAsync(IEnumerable<IExtensibleProject> projects, System.Threading.CancellationToken cancellationToken);
        IEnumerable<NuGet.Protocol.Core.Types.SourceRepository> AsLocalRepositories(IEnumerable<IExtensibleProject> projects);
    }
}
