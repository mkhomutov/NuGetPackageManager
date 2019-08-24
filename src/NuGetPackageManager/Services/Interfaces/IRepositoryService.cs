namespace NuGetPackageManager.Services
{
    using NuGet.Configuration;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Management;

    public interface IRepositoryService
    {
        SourceRepository GetRepository(PackageSource source);
        SourceContext AcquireContext(PackageSource source);
        SourceContext AcquireContext();

    }
}
