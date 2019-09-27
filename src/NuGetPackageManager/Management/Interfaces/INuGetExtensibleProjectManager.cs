using Catel;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGetPackageManager.Management.EventArgs;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management
{
    public interface INuGetExtensibleProjectManager
    {
        Task InstallPackageForProject(IExtensibleProject project, PackageIdentity package, CancellationToken token);

        Task UninstallPackageForProject(IExtensibleProject project, PackageIdentity package, CancellationToken token);
        Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(IExtensibleProject project, CancellationToken token);
        Task<bool> IsPackageInstalledAsync(IExtensibleProject project, PackageIdentity package, CancellationToken token);
        Task<Packaging.PackageCollection> CreatePackagesCollectionFromProjectsAsync(IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken);
        IEnumerable<NuGet.Protocol.Core.Types.SourceRepository> AsLocalRepositories(IEnumerable<IExtensibleProject> projects);

        event AsyncEventHandler<InstallNuGetProjectEventArgs> Install;

        event AsyncEventHandler<UninstallNuGetProjectEventArgs> Uninstall;

        event AsyncEventHandler<UpdateNuGetProjectEventArgs> Update;
    }
}
