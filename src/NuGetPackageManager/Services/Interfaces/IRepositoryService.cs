namespace NuGetPackageManager.Services
{
    using NuGet.Configuration;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Management;
    using System.Collections.Generic;

    public interface IRepositoryService
    {
        SourceRepository AcquireContext(PackageSource source, out SourceContext context);
    }
}
