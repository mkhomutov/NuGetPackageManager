using Catel;
using Catel.Logging;
using NuGet.Frameworks;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Management;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public async Task Install(PackageIdentity identity, IExtensibleProject project, CancellationToken cancellationToken)
        {
            var repositories = SourceContext.CurrentContext.Repositories;
            NuGetFramework targetFramework;

            try
            {
                targetFramework = NuGetFramework.ParseFrameworkName(project.Framework, _frameworkNameProvider);
            }
            catch(ArgumentException e)
            {
                Log.Error(e, "Incorrect target framework");
                return;
            }

            using (var cacheContext = new SourceCacheContext())
            {
                foreach (var r in repositories)
                {
                    var dependencyInfoResource = await r.GetResourceAsync<DependencyInfoResource>();

                    var dependencyInfo = dependencyInfoResource.ResolvePackage(identity, targetFramework, cacheContext, new Loggers.DebugLogger(true), cancellationToken);
                }
            }
        }
    }
}
