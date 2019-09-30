﻿namespace NuGetPackageManager.Services
{
    using Catel;
    using Catel.IoC;
    using Catel.Logging;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.Packaging.Signing;
    using NuGet.ProjectManagement;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Resolver;
    using NuGet.Versioning;
    using NuGetPackageManager.Cache;
    using NuGetPackageManager.Loggers;
    using NuGetPackageManager.Management;
    using NuGetPackageManager.Management.Exceptions;
    using System;
    using System.Collections.Generic;
    using System.IO;
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

        private readonly INuGetCacheManager _nuGetCacheManager;

        private readonly ITypeFactory _typeFactory;


        public PackageInstallationService(IFrameworkNameProvider frameworkNameProvider,
            ISourceRepositoryProvider sourceRepositoryProvider,
            IFileDirectoryService fileDirectoryService,
            ITypeFactory typeFactory)
        {
            Argument.IsNotNull(() => frameworkNameProvider);
            Argument.IsNotNull(() => sourceRepositoryProvider);
            Argument.IsNotNull(() => fileDirectoryService);
            //Argument.IsNotNull(() => nuGetCacheManager);

            _frameworkNameProvider = frameworkNameProvider;
            _sourceRepositoryProvider = sourceRepositoryProvider;
            _fileDirectoryService = fileDirectoryService;

            _typeFactory = typeFactory;

            _nuGetCacheManager = new NuGetCacheManager(_fileDirectoryService);
        }

        public async Task InstallAsync(PackageIdentity package, IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken)
        {
            try
            {
                var repositories = SourceContext.CurrentContext.Repositories;

                foreach (var proj in projects)
                {
                    await InstallAsync(package, proj, repositories, cancellationToken);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UninstallAsync(PackageIdentity package, IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken)
        {
            try
            {
                foreach (var proj in projects)
                {
                    await UninstallAsync(package, proj, cancellationToken);
                }
            }
            catch (Exception)
            {
                throw;
            }

        }


        public async Task UninstallAsync(PackageIdentity package, IExtensibleProject project, CancellationToken cancellationToken)
        {
            List<string> failedEntries = null;

            try
            {
                var folderProject = new FolderNuGetProject(project.ContentPath);

                if (folderProject.PackageExists(package))
                {
                    _fileDirectoryService.DeleteDirectoryTree(folderProject.GetInstalledPath(package), out failedEntries);
                }
            }
            catch (IOException e)
            {
                Log.Error(e, "Package files cannot be complete deleted by unexpected error (may be directory in use by another process?");
            }
            finally
            {
                LogHelper.LogUnclearedPaths(failedEntries, Log);
                LogHelper.LogUnclearedPaths(failedEntries, Log);
            }
        }


        public async Task<IDictionary<SourcePackageDependencyInfo, DownloadResourceResult>> InstallAsync(
            PackageIdentity identity,
            IExtensibleProject project,
            IReadOnlyList<SourceRepository> repositories,
            CancellationToken cancellationToken)
        {
            try
            {
                var targetFramework = FrameworkParser.TryParseFrameworkName(project.Framework, _frameworkNameProvider);

                var resContext = new NuGet.PackageManagement.ResolutionContext();

                var respos = _sourceRepositoryProvider.GetRepositories();

                var availabePackageStorage = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);

                using (var cacheContext = _nuGetCacheManager.GetCacheContext())
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

                using (var cacheContext = _nuGetCacheManager.GetCacheContext())
                {
                    var downloadResults = await DownloadPackagesResourcesAsync(availablePackagesToInstall, cacheContext, cancellationToken);

                    var extractionContext = GetExtractionContext();

                    await ExtractPackagesResourcesAsync(downloadResults, project, extractionContext, cancellationToken);

                    return downloadResults;
                }
            }
            catch (ProjectInstallException)
            {
                throw;
            }
            catch (Exception e)
            {
                Log.Error(e);
                throw;
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

        private async Task<IDictionary<SourcePackageDependencyInfo, DownloadResourceResult>> DownloadPackagesResourcesAsync(
            IEnumerable<SourcePackageDependencyInfo> packageIdentities, SourceCacheContext cacheContext, CancellationToken cancellationToken)
        {
            try
            {
                var downloaded = new Dictionary<SourcePackageDependencyInfo, DownloadResourceResult>();

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

                    downloaded.Add(package, downloadResult);
                }

                return downloaded;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task ExtractPackagesResourcesAsync(
            IDictionary<SourcePackageDependencyInfo, DownloadResourceResult> packageResources,
            IExtensibleProject project,
            PackageExtractionContext extractionContext,
            CancellationToken cancellationToken)
        {
            List<PackageIdentity> extractedPackages = new List<PackageIdentity>();

            try
            {


                var pathResolver = new PackagePathResolver(project.ContentPath);

                foreach (var packageResource in packageResources)
                {
                    var downloadedPart = packageResource.Value;
                    var packageIdentity = packageResource.Key;

                    var nupkgPath = pathResolver.GetInstalledPackageFilePath(packageIdentity);

                    bool alreadyInstalled = Directory.Exists(nupkgPath);

                    if (alreadyInstalled)
                    {
                        Log.Info($"Package {packageIdentity} already location in extraction directory");
                    }

                    Log.Info($"Extracting package {downloadedPart.GetResourceRoot()} to {project} project folder..");

                    var extractedPaths = await PackageExtractor.ExtractPackageAsync(
                        downloadedPart.PackageSource,
                        downloadedPart.PackageStream,
                        pathResolver,
                        extractionContext,
                        cancellationToken
                    );

                    Log.Info($"Successfully unpacked {extractedPaths.Count()} files");

                    if (!alreadyInstalled)
                    {
                        extractedPackages.Add(packageIdentity);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, $"An error occured during package extraction");

                var extractionEx = new ProjectInstallException(e.Message, e);

                extractionEx.CurrentBatch = extractedPackages;

                throw extractionEx;
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


        private async Task CheckLibAndFrameworkItems(IDictionary<SourcePackageDependencyInfo, DownloadResourceResult> downloadedPackagesDictionary,
            NuGetFramework targetFramework, CancellationToken cancellationToken)
        {
            var frameworkReducer = new FrameworkReducer();

            foreach (var package in downloadedPackagesDictionary.Keys)
            {
                var packageReader = downloadedPackagesDictionary[package].PackageReader;

                var libraries = await packageReader.GetLibItemsAsync(cancellationToken);

                var bestMatches = frameworkReducer.GetNearest(targetFramework, libraries.Select(x => x.TargetFramework));

                var frameworkItems = await packageReader.GetFrameworkItemsAsync(cancellationToken);

                var nearestFrameworkItems = frameworkReducer.GetNearest(targetFramework, frameworkItems.Select(x => x.TargetFramework));

                foreach (var libItemGroup in libraries)
                {
                    var satelliteFiles = await GetSatelliteFilesForLibrary(libItemGroup, packageReader, cancellationToken);
                }
            }
        }

        private async Task<List<string>> GetSatelliteFilesForLibrary(FrameworkSpecificGroup libraryFrameworkSpecificGroup, PackageReaderBase packageReader,
            CancellationToken cancellationToken)
        {
            var satelliteFiles = new List<string>();

            var nuspec = await packageReader.GetNuspecAsync(cancellationToken);
            var nuspecReader = new NuspecReader(nuspec);


            var satelliteFilesInGroup = libraryFrameworkSpecificGroup.Items
            .Where(item =>
                Path.GetDirectoryName(item)
                    .Split(Path.DirectorySeparatorChar)
                    .Contains(nuspecReader.GetLanguage(), StringComparer.OrdinalIgnoreCase)).ToList();

            satelliteFiles.AddRange(satelliteFilesInGroup);

            return satelliteFiles;
        }
    }
}
