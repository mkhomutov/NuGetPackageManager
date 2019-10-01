namespace NuGetPackageManager.Management
{
    using Catel;
    using Catel.Logging;
    using NuGet.Configuration;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.ProjectManagement;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Management.EventArgs;
    using NuGetPackageManager.Packaging;
    using NuGetPackageManager.Providers;
    using NuGetPackageManager.Services;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class NuGetExtensibleProjectManager : INuGetExtensibleProjectManager
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<IExtensibleProject, NuGetProjectMetadata> _storedProjectMetadata = new Dictionary<IExtensibleProject, NuGetProjectMetadata>();

        private readonly IPackageInstallationService _packageInstallationService;
        private readonly IFrameworkNameProvider _frameworkNameProvider;
        private readonly INuGetProjectContextProvider _nuGetProjectContextProvider;
        private readonly ISourceRepositoryProvider _repositoryProvider;

        const string MetadataTargetFramework = "TargetFramework";
        const string MetadataName = "Name";

        private BatchOperationToken batchToken;

        public NuGetExtensibleProjectManager(IPackageInstallationService packageInstallationService, IFrameworkNameProvider frameworkNameProvider,
            INuGetProjectContextProvider nuGetProjectContextProvider, ISourceRepositoryProvider repositoryProvider)
        {
            Argument.IsNotNull(() => packageInstallationService);
            Argument.IsNotNull(() => frameworkNameProvider);
            Argument.IsNotNull(() => nuGetProjectContextProvider);
            Argument.IsNotNull(() => repositoryProvider);

            _packageInstallationService = packageInstallationService;
            _frameworkNameProvider = frameworkNameProvider;
            _nuGetProjectContextProvider = nuGetProjectContextProvider;
            _repositoryProvider = repositoryProvider;
        }

        public event AsyncEventHandler<InstallNuGetProjectEventArgs> Install;

        async Task OnInstallAsync(IExtensibleProject project, PackageIdentity package, bool result)
        {
            var args = new InstallNuGetProjectEventArgs(project, package, result);

            if (!batchToken.IsDisposed)
            {
                batchToken.Add(new BatchedInstallNuGetProjectEventArgs(args));
                return;
            }

            await Install.SafeInvokeAsync(this, args);
        }

        public event AsyncEventHandler<UninstallNuGetProjectEventArgs> Uninstall;

        async Task OnUninstallAsync(IExtensibleProject project, PackageIdentity package, bool result)
        {
            var args = new UninstallNuGetProjectEventArgs(project, package, result);

            if (!batchToken.IsDisposed)
            {
                batchToken.Add(new BatchedUninstallNuGetProjectEventArgs(args));
                return;
            }

            await Uninstall.SafeInvokeAsync(this, args);
        }

        public event AsyncEventHandler<UpdateNuGetProjectEventArgs> Update;

        async Task OnUpdateAsync(IExtensibleProject project, PackageIdentity package)
        {
            var args = new UpdateNuGetProjectEventArgs(project, package);

            //if (!batchToken.IsDisposed)
            //{
            //    batchToken.Add(args);
            //    return;
            //}

            await Update.SafeInvokeAsync(this, args);
        }

        public async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(IExtensibleProject project, CancellationToken token)
        {
            try
            {
                //TODO should local metadata is also be checked?

                //var pathResolver = new PackagePathResolver(project.ContentPath);

                //var directories = System.IO.Directory.GetDirectories(project.ContentPath);

                var packageConfigProject = CreatePackageConfigProjectFromExtensible(project);

                var packageReferences = await packageConfigProject.GetInstalledPackagesAsync(token);

                return packageReferences;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Creates cross-project collection of installed package references
        /// grouped by id and version
        /// </summary>
        /// <param name="projects"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<PackageCollection> CreatePackagesCollectionFromProjectsAsync(IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken)
        {
            // Read package references from all projects.
            var tasks = projects
                .Select(project => GetInstalledPackagesAsync(project, cancellationToken));
            var packageReferences = await Task.WhenAll(tasks);

            // Group all package references for an id/version into a single item.
            var packages = packageReferences
                    .SelectMany(e => e)
                    .GroupBy(e => e.PackageIdentity, (key, group) => new PackageCollectionItem(key.Id, key.Version, group))
                    .ToArray();

            return new PackageCollection(packages);
        }

        /// <summary>
        /// Checks is package installed from records in config repository
        /// </summary>
        /// <param name="project"></param>
        /// <param name="package"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<bool> IsPackageInstalledAsync(IExtensibleProject project, PackageIdentity package, CancellationToken token)
        {
            //var underluyingFolderProject = new FolderNuGetProject(project.ContentPath);

            //var result = underluyingFolderProject.PackageExists(package);

            var installedReferences = await GetInstalledPackagesAsync(project, token);

            var installedPackage = installedReferences.Where(x => x.PackageIdentity.Equals(package, NuGet.Versioning.VersionComparison.Version)).FirstOrDefault();

            return installedPackage != null;
        }


        public async Task InstallPackageForProject(IExtensibleProject project, PackageIdentity package, CancellationToken token)
        {
            try
            {
                var packageConfigProject = CreatePackageConfigProjectFromExtensible(project);
                var repositories = SourceContext.CurrentContext.Repositories;

                var installerResults = await _packageInstallationService.InstallAsync(package, project, repositories, token);

                bool dependencyInstallResult = true;

                foreach (var packageDownloadResultPair in installerResults)
                {
                    var dependencyIdentity = packageDownloadResultPair.Key;
                    var downloadResult = packageDownloadResultPair.Value;

                    var result = await packageConfigProject.InstallPackageAsync(
                        dependencyIdentity,
                        downloadResult,
                        _nuGetProjectContextProvider.GetProjectContext(FileConflictAction.PromptUser),
                        token);

                    if (!result)
                    {
                        Log.Error($"Saving package configuration failed in project {project} when installing package {package}");
                    }

                    dependencyInstallResult &= result;
                }

                await OnInstallAsync(project, package, dependencyInstallResult);
            }
            catch (ProjectInstallException e)
            {
                Log.Error(e, $"The Installation of package {package} was failed");

                //rollback packages
                foreach (var canceledPackages in e.CurrentBatch)
                {
                    await UninstallPackageForProject(project, canceledPackages, token);
                }

            }
            catch (Exception e)
            {
                Log.Error(e, $"The Installation of package {package} was failed");
            }
        }

        public async Task InstallPackageForMultipleProject(IReadOnlyList<IExtensibleProject> projects, PackageIdentity package, CancellationToken token)
        {
            using (batchToken = new BatchOperationToken())
            {
                foreach (var project in projects)
                {
                    await InstallPackageForProject(project, package, token);
                }
            }

            //raise supressed events
            foreach (var args in batchToken.GetInvokationList<InstallNuGetProjectEventArgs>())
            {
                await Install.SafeInvokeAsync(this, args);
            }
        }

        public async Task UninstallPackageForProject(IExtensibleProject project, PackageIdentity package, CancellationToken token)
        {
            try
            {
                var packageConfigProject = CreatePackageConfigProjectFromExtensible(project);

                await _packageInstallationService.UninstallAsync(package, project, token);

                var isRepositoryConfigRecordExist = await IsPackageInstalledAsync(project, package, token);


                if (isRepositoryConfigRecordExist)
                {
                    var result = await packageConfigProject.UninstallPackageAsync(package, _nuGetProjectContextProvider.GetProjectContext(FileConflictAction.PromptUser), token);

                    if (!result)
                    {
                        Log.Error($"Saving package configuration failed in project {project} when installing package {package}");
                    }
                }

                await OnUninstallAsync(project, package, isRepositoryConfigRecordExist);
            }
            catch (Exception e)
            {
                Log.Error(e, $"Uninstall of package {package} was failed");
            }
        }

        public async Task UninstallPackageForMultipleProject(IReadOnlyList<IExtensibleProject> projects, PackageIdentity package, CancellationToken token)
        {
            using (batchToken = new BatchOperationToken())
            {
                foreach (var project in projects)
                {
                    await UninstallPackageForProject(project, package, token);
                }
            }

            //raise supressed events
            foreach (var args in batchToken.GetInvokationList<UninstallNuGetProjectEventArgs>())
            {
                await Uninstall.SafeInvokeAsync(this, args);
            }
        }

        /// <summary>
        /// Creates minimal required metadata for initializing NuGet PackagesConfigNuGetProject from
        /// our IExtensibleProject
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public PackagesConfigNuGetProject CreatePackageConfigProjectFromExtensible(IExtensibleProject project)
        {
            NuGetProjectMetadata metadata = null;

            if (!_storedProjectMetadata.TryGetValue(project, out metadata))
            {
                metadata = new NuGetProjectMetadata();

                var targetFramework = FrameworkParser.TryParseFrameworkName(project.Framework, _frameworkNameProvider);

                metadata.Data.Add(MetadataTargetFramework, targetFramework);
                metadata.Data.Add(MetadataName, project.Name);

                _storedProjectMetadata.Add(project, metadata);
            }

            var packagesConfigProject = new PackagesConfigNuGetProject(project.ContentPath, metadata.Data);

            return packagesConfigProject;
        }

        public IEnumerable<SourceRepository> AsLocalRepositories(IEnumerable<IExtensibleProject> projects)
        {
            var repos = projects.Select(x => _repositoryProvider
                .CreateRepository(
                        new PackageSource(x.ContentPath), NuGet.Protocol.FeedType.FileSystemV2
                ));

            return repos;
        }

        private class BatchOperationToken : IDisposable
        {
            private readonly List<NuGetProjectEventArgs> supressedInvokationEventArgs = new List<NuGetProjectEventArgs>();

            public void Add(NuGetProjectEventArgs eventArgs)
            {
                supressedInvokationEventArgs.Add(eventArgs);
            }

            public bool IsDisposed { get; private set; }

            public IEnumerable<T> GetInvokationList<T>() where T : NuGetProjectEventArgs
            {
                if (supressedInvokationEventArgs.All(args => args is T))
                {
                    return supressedInvokationEventArgs.Cast<T>();
                }

                Log.Warning("Mixed batched event args");
                return Enumerable.Empty<T>();
            }

            public void Dispose()
            {
                var last = supressedInvokationEventArgs.LastOrDefault();

                if (last != null)
                {
                    switch (last)
                    {
                        case BatchedInstallNuGetProjectEventArgs b:
                                b.IsBatchEnd = true;
                                break;

                        case BatchedUninstallNuGetProjectEventArgs b:
                                b.IsBatchEnd = true;
                                break;
                    }
                }

                IsDisposed = true;
            }
        }
    }
}
