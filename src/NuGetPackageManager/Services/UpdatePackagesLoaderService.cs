namespace NuGetPackageManager.Services
{
    using Catel;
    using Catel.IoC;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Management;
    using NuGetPackageManager.Pagination;
    using NuGetPackageManager.Providers;
    using System;
    using System.Collections.Generic;
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

            _feedRepositoryLoader = new Lazy<IPackagesLoaderService>(() => _serviceLocator.ResolveType<IPackagesLoaderService>("Installed"));
            _projectRepositoryLoader = new Lazy<IPackagesLoaderService>(() => _serviceLocator.ResolveType<IPackagesLoaderService>("Updates"));
        }

        public Lazy<IPackageMetadataProvider> PackageMetadataProvider { get; set; }

        public async Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token)
        {
            var installedPackagesMetadatas = await _projectRepositoryLoader.Value.LoadAsync(searchTerm, pageContinuation, searchFilter, token);

            if (PackageMetadataProvider == null)
            {
                PackageMetadataProvider = _projectRepositoryLoader.Value.PackageMetadataProvider;
            }

            //getting last metadata from 
            return null;
        }


    }
}
