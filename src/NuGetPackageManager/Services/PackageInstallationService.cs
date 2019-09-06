using Catel;
using Catel.Logging;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Management;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public class PackageInstallationService : IPackageInstallationService
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IFrameworkNameProvider _frameworkNameProvider;

        public PackageInstallationService(IFrameworkNameProvider frameworkNameProvider)
        {
            Argument.IsNotNull(() => frameworkNameProvider);

            _frameworkNameProvider = frameworkNameProvider;
        }

        public async Task Install(PackageIdentity identity, IEnumerable<IExtensibleProject> projects, CancellationToken cancellationToken)
        {
            try
            {
                var repositories = SourceContext.CurrentContext.Repositories;

                foreach (var proj in projects)
                {
                    await Install(identity, proj, repositories, cancellationToken);
                }
            }
            catch(Exception)
            {
                throw;
            }
        }

        public async Task Install(PackageIdentity identity, IExtensibleProject project, IReadOnlyList<SourceRepository> repositories, CancellationToken cancellationToken)
        {
            var targetFramework = TryParseFrameworkName(project.Framework, _frameworkNameProvider);

            using (var cacheContext = new SourceCacheContext())
            {
                foreach (var repository in repositories)
                {
                    var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>();

                    var dependencyInfo = await dependencyInfoResource.ResolvePackage(
                        identity, targetFramework, cacheContext, new Loggers.DebugLogger(true), cancellationToken);
                }
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
    }
}
