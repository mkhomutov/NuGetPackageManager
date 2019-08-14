using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface IRepositoryService
    {
        SourceRepository GetOrCreateRepositoryForSource(PackageSource source);
        SourceRepository[] GetOrCreateRepositoryForSourceMultiple(IEnumerable<PackageSource> sources);

        SourceContext AcquireContext(PackageSource source);
        void ReleaseContext(SourceContext context);
        
        /// <summary>
        /// Returns current context repositories
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<SourceRepository> GetContextRepositories();
    }
}
