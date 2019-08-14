using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public class RepositoryService : IRepositoryService
    {
        public SourceRepository GetOrCreateRepositoryForSource(PackageSource source)
        {
            //todo store repos
            return new SourceRepository(source,  Repository.Provider.GetCoreV3());
        }

        public SourceRepository[] GetOrCreateRepositoryForSourceMultiple(IEnumerable<PackageSource> sources)
        {
            return sources.Select(s => new SourceRepository(s, Repository.Provider.GetCoreV3())).ToArray();
        }
    }
}
