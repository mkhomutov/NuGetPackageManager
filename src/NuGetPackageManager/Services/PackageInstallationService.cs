namespace NuGetPackageManager.Services
{
    using Catel;
    using Catel.Logging;
    using NuGet.Common;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Resolver;
    using NuGet.Versioning;
    using NuGetPackageManager.Loggers;
    using NuGetPackageManager.Management;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PackageInstallationService : IPackageInstallationService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private static readonly ILogger NuGetLog = new DebugLogger(true);

        private readonly IFrameworkNameProvider _frameworkNameProvider;

        private readonly ISourceRepositoryProvider _sourceRepositoryProvider;

        public PackageInstallationService(IFrameworkNameProvider frameworkNameProvider,
            ISourceRepositoryProvider sourceRepositoryProvider)
        {
            Argument.IsNotNull(() => frameworkNameProvider);
            Argument.IsNotNull(() => sourceRepositoryProvider);

            _frameworkNameProvider = frameworkNameProvider;
            _sourceRepositoryProvider = sourceRepositoryProvider;
        }

        public async Task InstallAsync(PackageIdentity identity, IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken)
        {
            try
            {
                var repositories = SourceContext.CurrentContext.Repositories;

                foreach (var proj in projects)
                {
                    await InstallAsync(identity, proj, repositories, cancellationToken);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task InstallAsync(PackageIdentity identity, IExtensibleProject project, IReadOnlyList<SourceRepository> repositories, CancellationToken cancellationToken)
        {
            var targetFramework = TryParseFrameworkName(project.Framework, _frameworkNameProvider);

            var settings = new NuGet.Configuration.NullSettings();

            var packageManager = new NuGet.PackageManagement.NuGetPackageManager(_sourceRepositoryProvider, settings, @"D:\Dev\NuGetTest");

            var resContext = new NuGet.PackageManagement.ResolutionContext();

            var respos = _sourceRepositoryProvider.GetRepositories();

            var availabePackageStorage = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

            using (var cacheContext = NullSourceCacheContext.Instance)
            {
                foreach (var repository in repositories)
                {
                    var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>();
                    var regResource = await repository.GetResourceAsync<RegistrationResourceV3>();

                    var uri = regResource.GetUri(identity.Id);

                    await ResolvePackagesRecursivelyAsync(identity, targetFramework, dependencyInfoResource, cacheContext,
                        availabePackageStorage, cancellationToken);

                }
            }

            var resolverContext = GetPackageContext(identity, availabePackageStorage);

            var resolver = new PackageResolver();

            var packagesInstallationList = resolver.Resolve(resolverContext, cancellationToken);

            var availablePackagesToInstall = packagesInstallationList
                .Select(
                    x => availabePackageStorage
                        .Single(p => PackageIdentityComparer.Default.Equals(p, x)));

            using (var cacheContext = NullSourceCacheContext.Instance)
            {
                var downloaded = await DownloadDependencyGraphAsync(availablePackagesToInstall, cacheContext, cancellationToken);
            }
        }

        private async Task ResolvePackagesRecursivelyAsync(PackageIdentity identity, NuGetFramework targetFramework,
            DependencyInfoResource dependencyInfoResource,
            SourceCacheContext cacheContext,
            HashSet<SourcePackageDependencyInfo> storage,
            CancellationToken cancellationToken)
        {
            Argument.IsNotNull(() => storage);

            var logger = new Loggers.DebugLogger(true);

            HashSet<SourcePackageDependencyInfo> packageStore = storage;

            Stack<SourcePackageDependencyInfo> downloadStack = new Stack<SourcePackageDependencyInfo>();

            //get top dependency
            var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                            identity, targetFramework, cacheContext, logger, cancellationToken);

            if (dependencyInfo == null)
            {
                Log.Error($"Cannot resolve {identity} package for target framework {targetFramework}");
                return;
            }

            downloadStack.Push(dependencyInfo); //and add it to package store


            var singleVersion = new VersionRange(minVersion: identity.Version, includeMinVersion: true, maxVersion: identity.Version, includeMaxVersion: true);

            //commented code used for testing
            //var httpClient = typeof(DependencyInfoResourceV3).GetFieldEx("_client").GetValue(dependencyInfoResource);
            //var regInfo = await ResolverMetadataClient.GetRegistrationInfo(httpClient as HttpSource, testUri, identity.Id, singleVersion, cacheContext, targetFramework, logger, cancellationToken);

            while (downloadStack.Count > 0)
            {
                var rootDependency = downloadStack.Pop();

                //store all new packges
                if (!packageStore.Contains(rootDependency))
                {
                    packageStore.Add(rootDependency);
                }
                else
                {
                    continue;
                }

                foreach (var dependency in rootDependency.Dependencies)
                {
                    //currently we using restricted version during child dependency resolving 
                    //but possibly it should be configured in project
                    var relatedIdentity = new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion);

                    var relatedDepInfo = await dependencyInfoResource.ResolvePackage(relatedIdentity, targetFramework, cacheContext, logger, cancellationToken);

                    downloadStack.Push(relatedDepInfo);
                }
            }
        }

        private async Task<IReadOnlyList<DownloadResourceResult>> DownloadDependencyGraphAsync(IEnumerable<SourcePackageDependencyInfo> packageIdentities, SourceCacheContext cacheContext, CancellationToken cancellationToken)
        {
            var downloaded = new List<DownloadResourceResult>();

            foreach (var install in packageIdentities)
            {
                var downloadResource = await install.Source.GetResourceAsync<DownloadResource>(cancellationToken);

                var downloadResult = await downloadResource.GetDownloadResourceResultAsync
                    (
                        install,
                        new PackageDownloadContext(cacheContext),
                        Constants.DefaultGlobalPackageCacheFolder,
                        NuGetLog,
                        cancellationToken
                    );


                downloaded.Add(downloadResult);
            }

            return downloaded;
        }

        private NuGetFramework TryParseFrameworkName(string frameworkString, IFrameworkNameProvider frameworkNameProvider)
        {
            try
            {
                return NuGetFramework.ParseFrameworkName(frameworkString, frameworkNameProvider);
            }
            catch (ArgumentException e)
            {
                Log.Error(e, "Incorrect target framework");
                throw;
            }
        }

        private PackageResolverContext GetPackageContext(PackageIdentity package, IEnumerable<SourcePackageDependencyInfo> flatDependencies)
        {
            var idArray = new[] { package.Id };

            var requiredPackages = Enumerable.Empty<string>();

            var packagesConfig = Enumerable.Empty<PackageReference>();

            var prefferedVersion = Enumerable.Empty<PackageIdentity>();

            var resolverContext = new PackageResolverContext(
                DependencyBehavior.Lowest,
                idArray,
                requiredPackages,
                packagesConfig,
                prefferedVersion,
                flatDependencies,
                _sourceRepositoryProvider.GetRepositories().Select(x => x.PackageSource),
                NuGetLog
            );

            return resolverContext;
        }
    }
}
