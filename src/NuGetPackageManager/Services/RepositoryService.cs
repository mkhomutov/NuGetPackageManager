namespace NuGetPackageManager.Services
{
    using NuGet.Configuration;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Management;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RepositoryService : IRepositoryService
    {
        public SourceContext AcquireContext(PackageSource source)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<SourceRepository> GetContextRepositories()
        {
            throw new NotImplementedException();
        }

        public SourceRepository GetOrCreateRepositoryForSource(PackageSource source)
        {
            //todo store repos
            return new SourceRepository(source, Repository.Provider.GetCoreV3());
        }

        public SourceRepository[] GetOrCreateRepositoryForSourceMultiple(IEnumerable<PackageSource> sources)
        {
            return sources.Select(s => new SourceRepository(s, Repository.Provider.GetCoreV3())).ToArray();
        }

        public void ReleaseContext(SourceContext context)
        {
            throw new NotImplementedException();
        }
    }
}
