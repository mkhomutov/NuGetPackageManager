namespace NuGetPackageManager.Services
{
    using NuGet.Packaging.Core;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPackageInstallationService
    {
        Task Install(PackageIdentity identity, IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken);
    }
}
