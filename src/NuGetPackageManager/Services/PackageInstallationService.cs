namespace NuGetPackageManager.Services
{
    using Catel;
    using Catel.Logging;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Packaging.Signing;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Resolver;
    using NuGet.Versioning;
    using NuGetPackageManager.Extensions;
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

        private readonly IFileDirectoryService _fileDirectoryService;

        public PackageInstallationService(IFrameworkNameProvider frameworkNameProvider,
            ISourceRepositoryProvider sourceRepositoryProvider,
            IFileDirectoryService fileDirectoryService)
        {
            Argument.IsNotNull(() => frameworkNameProvider);
            Argument.IsNotNull(() => sourceRepositoryProvider);
            Argument.IsNotNull(() => fileDirectoryService);

            _frameworkNameProvider = frameworkNameProvider;
            _sourceRepositoryProvider = sourceRepositoryProvider;
            _fileDirectoryService = fileDirectoryService;
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

            //var packageManager = new NuGet.PackageManagement.NuGetPackageManager(_sourceRepositoryProvider, new NuGet.Configuration.NullSettings(), @"D:\Dev\NuGetTest");

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

                    await ResolveDependenciesRecursivelyAsync(identity, targetFramework, dependencyInfoResource, cacheContext,
                        availabePackageStorage, cancellationToken);

                }
            }

            var resolverContext = GetResolverContext(identity, availabePackageStorage);

            var resolver = new PackageResolver();

            var packagesInstallationList = resolver.Resolve(resolverContext, cancellationToken);

            var availablePackagesToInstall = packagesInstallationList
                .Select(
                    x => availabePackageStorage
                        .Single(p => PackageIdentityComparer.Default.Equals(p, x)));

            using (var cacheContext = NullSourceCacheContext.Instance)
            {
                var downloadResults = await DownloadPackagesResourcesAsync(availablePackagesToInstall, cacheContext, cancellationToken);

                var extractionContext = GetExtractionContext();

                await ExtractPackagesResourcesAsync(downloadResults, project, extractionContext, cancellationToken);
            }
        }

        private async Task ResolveDependenciesRecursivelyAsync(PackageIdentity identity, NuGetFramework targetFramework,
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

        private async Task<IReadOnlyList<DownloadResourceResult>> DownloadPackagesResourcesAsync(
            IEnumerable<SourcePackageDependencyInfo> packageIdentities, SourceCacheContext cacheContext, CancellationToken cancellationToken)
        {
            try
            {
                var downloaded = new List<DownloadResourceResult>();

                string globalFolderPath = _fileDirectoryService.GetGlobalPackagesFolder();

                foreach (var package in packageIdentities)
                {
                    var downloadResource = await package.Source.GetResourceAsync<DownloadResource>(cancellationToken);

                    var downloadResult = await downloadResource.GetDownloadResourceResultAsync
                        (
                            package,
                            new PackageDownloadContext(cacheContext),
                            globalFolderPath,
                            NuGetLog,
                            cancellationToken
                        );


                    downloaded.Add(downloadResult);
                }

                return downloaded;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task ExtractPackagesResourcesAsync(
            IEnumerable<DownloadResourceResult> downloadResources, IExtensibleProject project, PackageExtractionContext extractionContext, CancellationToken cancellationToken)
        {
            var pathResolver = new PackagePathResolver(project.ContentPath);

            foreach (var downloadPart in downloadResources)
            {
                Log.Info($"Extracting package {downloadPart.GetResourceRoot()} to {project} project folder..");

                var extractedPaths = await PackageExtractor.ExtractPackageAsync(
                    downloadPart.PackageSource,
                    downloadPart.PackageStream,
                    pathResolver,
                    extractionContext,
                    cancellationToken
                );

                Log.Info($"Successfully unpacked {extractedPaths.Count()} files");
            }

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

        private PackageResolverContext GetResolverContext(PackageIdentity package, IEnumerable<SourcePackageDependencyInfo> flatDependencies)
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

        private PackageExtractionContext GetExtractionContext()
        {
            //todo provide read certs?
            var signaturesCerts = Enumerable.Empty<TrustedSignerAllowListEntry>().ToList();

            var policyContextForClient = ClientPolicyContext.GetClientPolicy(Settings.LoadDefaultSettings(null), NuGetLog);

            var extractionContext = new PackageExtractionContext(
                PackageSaveMode.Defaultv3,
                XmlDocFileSaveMode.Skip,
                policyContextForClient,
                NuGetLog
            );

            return extractionContext;
        }
    }
}
