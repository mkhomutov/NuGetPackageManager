namespace NuGetPackageManager.Providers
{
    using Catel;
    using Catel.Logging;
    using NuGet.Common;
    using NuGet.Packaging.Core;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Extensions;
    using NuGetPackageManager.Loggers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PackageMetadataProvider : IPackageMetadataProvider
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private static readonly ILogger NuGetLogger = new DebugLogger(true);

        private readonly IEnumerable<SourceRepository> _sourceRepositories;

        private readonly IEnumerable<SourceRepository> _optionalLocalRepositories;

        private readonly SourceRepository _localRepository;

        public PackageMetadataProvider(IEnumerable<SourceRepository> sourceRepositories,
            IEnumerable<SourceRepository> optionalGlobalLocalRepositories)
        {
            Argument.IsNotNull(() => sourceRepositories);

            _sourceRepositories = sourceRepositories;
            _optionalLocalRepositories = optionalGlobalLocalRepositories;
        }

        public PackageMetadataProvider(SourceRepository localRepository, IEnumerable<SourceRepository> sourceRepositories,
            IEnumerable<SourceRepository> optionalGlobalLocalRepositories) : this(sourceRepositories, optionalGlobalLocalRepositories)
        {
            Argument.IsNotNull(() => localRepository);

            _localRepository = localRepository;
        }

        public async Task<IPackageSearchMetadata> GetLocalPackageMetadataAsync(PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            var sources = new List<SourceRepository>();

            if (_optionalLocalRepositories != null)
            {
                sources.AddRange(_optionalLocalRepositories);
            }

            if (_sourceRepositories != null)
            {
                sources.AddRange(_sourceRepositories);
            }
  
            // Take the package from the first source it is found in
            foreach (var source in sources)
            {
                var result = await GetPackageMetadataFromLocalSourceAsync(source, identity, cancellationToken);

                if(result != null)
                {
                    //TODO why additional fetching needed?
                    //return result.WithVersions(
                    //    () => FetchAndMergeVersionsAsync(identity, includePrerelease, ))
                    return result;
                }
            }

            return null;
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
                    NuGetLogger,
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

                var package = await metadataResource?.GetMetadataAsync(identity, sourceCacheContext, NuGetLogger, cancellationToken);
                return package;
            }
        }

        private async Task<IPackageSearchMetadata> GetPackageMetadataFromLocalSourceAsync(SourceRepository localRepository, PackageIdentity packageIdentity, CancellationToken token)
        {
            var localResource = await localRepository.GetResourceAsync<PackageMetadataResource>(token);

            using (var sourceCacheContext = new SourceCacheContext())
            {
                var localPackages = await localResource?.GetMetadataAsync(
                    packageIdentity.Id,
                    includePrerelease: true,
                    includeUnlisted: true,
                    sourceCacheContext: sourceCacheContext,
                    log: NuGetLogger,
                    token: token);

                var packageMetadata = localPackages?.FirstOrDefault(p => p.Identity.Version == packageIdentity.Version);

                var versions = new[]
                {
                    new VersionInfo(packageIdentity.Version)
                };

                return packageMetadata?.WithVersions(versions);
            }
        }

        //TODO
        private async Task<IEnumerable<VersionInfo>> FetchAndMergeVersionsAsync(PackageIdentity identity, bool includePrerelease, CancellationToken token)
        {
            var rp = _localRepository;

            Log.Info(rp.ToString());
            return null;
        }
    }
}
