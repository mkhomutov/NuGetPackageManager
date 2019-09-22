namespace NuGetPackageManager.Management
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Catel;
    using Catel.Logging;
    using NuGet.Frameworks;
    using NuGet.Packaging;
    using NuGet.Packaging.Core;
    using NuGet.ProjectManagement;
    using NuGetPackageManager.Packaging;
    using NuGetPackageManager.Providers;
    using NuGetPackageManager.Services;

    public class NuGetExtensibleProjectManager : INuGetExtensibleProjectManager
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly Dictionary<IExtensibleProject, NuGetProjectMetadata> _storedProjectMetadata = new Dictionary<IExtensibleProject, NuGetProjectMetadata>();

        private readonly IPackageInstallationService _packageInstallationService;
        private readonly IFrameworkNameProvider _frameworkNameProvider;
        private readonly INuGetProjectContextProvider _nuGetProjectContextProvider;

        const string MetadataTargetFramework = "TargetFramework";
        const string MetadataName = "Name";


        public NuGetExtensibleProjectManager(IPackageInstallationService packageInstallationService, IFrameworkNameProvider frameworkNameProvider, 
            INuGetProjectContextProvider nuGetProjectContextProvider)
        {
            Argument.IsNotNull(() => packageInstallationService);
            Argument.IsNotNull(() => frameworkNameProvider);
            Argument.IsNotNull(() => nuGetProjectContextProvider);

            _packageInstallationService = packageInstallationService;
            _frameworkNameProvider = frameworkNameProvider;
            _nuGetProjectContextProvider = nuGetProjectContextProvider;
        }

        public async Task<IEnumerable<PackageReference>> GetInstalledPackagesAsync(IExtensibleProject project, CancellationToken token)
        {
            try
            {
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

        public async Task<bool> IsPackageInstalledAsync(IExtensibleProject project, PackageIdentity package, CancellationToken token)
        {
            //var underluyingFolderProject = new FolderNuGetProject(project.ContentPath);

            //var result = underluyingFolderProject.PackageExists(package);

            var installedReferences = await GetInstalledPackagesAsync(project, token);

            var installedPackage = installedReferences.FirstOrDefault(r => r.PackageIdentity == package);

            return installedPackage != null;
        }


        public async Task InstallPackageForProject(IExtensibleProject project, PackageIdentity package, CancellationToken token)
        {
            try
            {
                var packageConfigProject = CreatePackageConfigProjectFromExtensible(project);
                var repositories = SourceContext.CurrentContext.Repositories;

                var installerResults = await _packageInstallationService.InstallAsync(package, project, repositories, token);

                foreach (var packageDownloadResultPair in installerResults)
                {
                    var dependencyIdentity = packageDownloadResultPair.Key;
                    var downloadResult = packageDownloadResultPair.Value;

                    var result = packageConfigProject.InstallPackageAsync(package, downloadResult, _nuGetProjectContextProvider.GetProjectContext(FileConflictAction.PromptUser), token);
                }
            }
            catch(Exception e)
            {
                Log.Error(e, $"The Installation of Package {package} was failed");
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
            
            if(!_storedProjectMetadata.TryGetValue(project, out metadata))
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
    }
}
