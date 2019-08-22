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

        #region IPackageMetadataProvider

        public Task<IPackageSearchMetadata> GetLocalPackageMetadataAsync(PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<IPackageSearchMetadata> GetPackageMetadataAsync(PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            var tasks = _sourceRepositories
               .Select(r => GetPackageMetadataAsyncFromSource(r, identity, includePrerelease, cancellationToken)).ToArray();

            //if (_localRepository != null)
            //{
            //    tasks.Add(_localRepository.GetPackageMetadataFromLocalSourceAsync(identity, cancellationToken));
            //}

            var completed = (await tasks.WhenAllOrException()).Where(x => x.IsSuccess)
                .Select(x => x.UnwrapResult())
                .Where(metadata => metadata != null);


            var master = completed.FirstOrDefault(m => !string.IsNullOrEmpty(m.Summary))
                ?? completed.FirstOrDefault()
                ?? PackageSearchMetadataBuilder.FromIdentity(identity).Build();

            //return master.WithVersions(
            //    asyncValueFactory: () => MergeVersionsAsync(identity, completed));

            return master;
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadataListAsync(string packageId, bool includePrerelease, bool includeUnlisted, CancellationToken cancellationToken)
        {
            var tasks = _sourceRepositories.Select(repo => GetPackageMetadataListAsyncFromSource(repo, packageId, includePrerelease, includeUnlisted, cancellationToken)).ToArray();

            var completed = (await tasks.WhenAllOrException()).Where(x => x.IsSuccess).
                Select(x => x.UnwrapResult())
                .Where(metadata => metadata != null);

            var packages = completed.SelectMany(p => p);

            var uniquePackages = packages
                .GroupBy(
                   m => m.Identity.Version,
                   (v, ms) => ms.First());

            return uniquePackages;
        }

        #endregion

        private async Task<IEnumerable<IPackageSearchMetadata>> GetPackageMetadataListAsyncFromSource(SourceRepository repository, 
            string packageId, 
            bool includePrerelease,
            bool includeUnlisted,
            CancellationToken cancellationToken)
        {

            var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                // Update http source cache context MaxAge so that it can always go online to fetch
                // latest versions of the package.
                sourceCacheContext.MaxAge = DateTimeOffset.UtcNow;

                var packages = await metadataResource?.GetMetadataAsync(
                    packageId,
                    includePrerelease,
                    includeUnlisted,
                    sourceCacheContext,
                    new Loggers.DebugLogger(true),
                    cancellationToken);

                return packages;
            }
        }

        private async Task<IPackageSearchMetadata> GetPackageMetadataAsyncFromSource(SourceRepository repository, 
            PackageIdentity identity,
            bool includePrerelease, 
            CancellationToken cancellationToken)
        {
            var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>(cancellationToken);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                sourceCacheContext.MaxAge = DateTimeOffset.UtcNow;

                var package = await metadataResource?.GetMetadataAsync(identity, sourceCacheContext, new Loggers.DebugLogger(true), cancellationToken);
                return package;
            }
        }
    }
}
