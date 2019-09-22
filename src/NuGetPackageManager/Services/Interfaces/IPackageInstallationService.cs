namespace NuGetPackageManager.Services
{
    using NuGet.Packaging.Core;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPackageInstallationService
    {
        Task InstallAsync(PackageIdentity identity, IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken);
        Task<IDictionary<NuGet.Protocol.Core.Types.SourcePackageDependencyInfo, NuGet.Protocol.Core.Types.DownloadResourceResult>> InstallAsync(PackageIdentity identity, IExtensibleProject project, IReadOnlyList<NuGet.Protocol.Core.Types.SourceRepository> repositories, CancellationToken cancellationToken);
        Task UninstallAsync(PackageIdentity package, IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken);
        Task UninstallAsync(PackageIdentity package, IExtensibleProject project, CancellationToken cancellationToken);
    }
}
