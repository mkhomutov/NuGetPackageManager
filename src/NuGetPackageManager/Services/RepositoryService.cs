namespace NuGetPackageManager.Services
{
    using Catel;
    using NuGet.Configuration;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Management;
    using NuGetPackageManager.Models;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class RepositoryService : IRepositoryService
    {
        private readonly ISourceRepositoryProvider _sourceRepositoryProvider;
        private readonly Dictionary<PackageSource, SourceRepository> _constructedRepositories = new Dictionary<PackageSource, SourceRepository>();


        public RepositoryService(ISourceRepositoryProvider sourceRepositoryProvider)
        {
            Argument.IsNotNull(() => sourceRepositoryProvider);

            _sourceRepositoryProvider = sourceRepositoryProvider;
        }

        public SourceContext AcquireContext(PackageSource source)
        {
            SourceRepository sourceRepo = null;

            if (!_constructedRepositories.TryGetValue(source, out sourceRepo))
            {
                sourceRepo = _sourceRepositoryProvider.CreateRepository(source);
            }

            var context = new SourceContext(new List<SourceRepository>() { sourceRepo }, this);

            return context;
        }

        public SourceContext AcquireContext()
        {
            //acquire for all by default
            IReadOnlyList<SourceRepository> repos = _sourceRepositoryProvider.GetRepositories().ToList();

            var context = new SourceContext(repos, this);

            return context;
        }
    }
}
