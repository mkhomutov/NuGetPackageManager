using Catel;
using Catel.Logging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Extensions;
using NuGetPackageManager.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Providers
{
    public class PackageMetadataProvider : IPackageMetadataProvider
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IEnumerable<SourceRepository> _sourceRepositories;
        private readonly SourceRepository _optionalLocalRepository;

        public PackageMetadataProvider(IEnumerable<SourceRepository> sourceRepositories,
            SourceRepository optionalLocalRepository)
        {
            Argument.IsNotNull(() => sourceRepositories);

            _sourceRepositories = sourceRepositories;
            _optionalLocalRepository = optionalLocalRepository;
        }

        public Task<IPackageSearchMetadata> GetLocalPackageMetadataAsync(PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            //var tasks = _sourceRepositories.Select(repo => repo.Get)
            throw new NotImplementedException();
        }

        public Task<IPackageSearchMetadata> GetPackageMetadataAsync(PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadataListAsync(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken cancellationToken)
        {
            //todo load metadata details
 
            var tasks = _sourceRepositories.Select(repo => GetPackageMetadataListAsync(repo, packageId, includePrerelease, includeUnlisted, cancellationToken)).ToArray();

            var completed = (await tasks.WhenAllOrException()).Where(x => x.IsSuccess).Select(x => x.UnwrapResult());

            var packages = completed.SelectMany(p => p);

            var uniquePackages = packages
                .GroupBy(
                   m => m.Identity.Version,
                   (v, ms) => ms.First());

            throw new NotImplementedException();
        }

        private async Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadataListAsync(SourceRepository repository, 
            string packageId, 
            bool includePrerelease,
            bool includeUnlisted,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
