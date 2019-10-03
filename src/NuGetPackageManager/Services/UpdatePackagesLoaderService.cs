namespace NuGetPackageManager.Services
{
    using Catel;
    using Catel.IoC;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Management;
    using NuGetPackageManager.Packaging;
    using NuGetPackageManager.Pagination;
    using NuGetPackageManager.Providers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class UpdatePackagesLoaderService : IPackagesLoaderService
    {
        private readonly IRepositoryService _repositoryService;
        private readonly IExtensibleProjectLocator _extensibleProjectLocator;
        private readonly INuGetExtensibleProjectManager _nuGetExtensibleProjectManager;

        private readonly IServiceLocator _serviceLocator;

        //underlying service
        private readonly Lazy<IPackagesLoaderService> _feedRepositoryLoader;
        private readonly Lazy<IPackagesLoaderService> _projectRepositoryLoader;

        public UpdatePackagesLoaderService(IRepositoryService repositoryService, IExtensibleProjectLocator extensibleProjectLocator,
           INuGetExtensibleProjectManager nuGetExtensibleProjectManager)
        {
            Argument.IsNotNull(() => repositoryService);
            Argument.IsNotNull(() => extensibleProjectLocator);
            Argument.IsNotNull(() => nuGetExtensibleProjectManager);

            _repositoryService = repositoryService;
            _extensibleProjectLocator = extensibleProjectLocator;
            _nuGetExtensibleProjectManager = nuGetExtensibleProjectManager;

            _serviceLocator = this.GetServiceLocator();

            _feedRepositoryLoader = new Lazy<IPackagesLoaderService>(() => _serviceLocator.ResolveType<IPackagesLoaderService>());
            _projectRepositoryLoader = new Lazy<IPackagesLoaderService>(() => _serviceLocator.ResolveType<IPackagesLoaderService>("Installed"));
        }

        public Lazy<IPackageMetadataProvider> PackageMetadataProvider { get; set; }

        public async Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token)
        {
            var installedPackagesMetadatas = await _projectRepositoryLoader.Value.LoadAsync(searchTerm, pageContinuation, searchFilter, token);

            if (PackageMetadataProvider == null)
            {
                PackageMetadataProvider = _projectRepositoryLoader.Value.PackageMetadataProvider;
            }

            List<IPackageSearchMetadata> updateList = new List<IPackageSearchMetadata>();

            //getting last metadata
            foreach (var package in installedPackagesMetadatas)
            {
                var clonedMetadata = await PackageMetadataProvider.Value.GetPackageMetadataAsync(package.Identity, searchFilter.IncludePrerelease, token);

                if (clonedMetadata == null)
                {
                    continue;
                }

                var versions = await clonedMetadata.GetVersionsAsync();

                
                if (versions.FirstOrDefault().Version > package.Identity.Version)
                {
                    updateList.Add(clonedMetadata);
                }
            }

            return updateList;
        }


    }
}
