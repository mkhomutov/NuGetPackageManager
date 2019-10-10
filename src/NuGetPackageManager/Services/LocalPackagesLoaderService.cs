﻿namespace NuGetPackageManager.Services
{
    using Catel;
    using NuGet.Packaging.Core;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGet.Versioning;
    using NuGetPackageManager.Management;
    using NuGetPackageManager.Pagination;
    using NuGetPackageManager.Providers;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class LocalPackagesLoaderService : IPackagesLoaderService
    {
        private readonly IExtensibleProjectLocator _extensibleProjectLocator;

        private readonly INuGetExtensibleProjectManager _projectManager;

        private readonly IRepositoryService _repositoryService;

        public Lazy<IPackageMetadataProvider> PackageMetadataProvider { get; set; }

        public LocalPackagesLoaderService(IRepositoryService repositoryService, IExtensibleProjectLocator extensibleProjectLocator,
            INuGetExtensibleProjectManager nuGetExtensibleProjectManager)
        {
            Argument.IsNotNull(() => extensibleProjectLocator);
            Argument.IsNotNull(() => nuGetExtensibleProjectManager);
            Argument.IsNotNull(() => repositoryService);

            _extensibleProjectLocator = extensibleProjectLocator;
            _projectManager = nuGetExtensibleProjectManager;
            _repositoryService = repositoryService;

            PackageMetadataProvider = new Lazy<IPackageMetadataProvider>(() => InitializeMetdataProvider());
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token)
        {
            Argument.IsValid(nameof(pageContinuation), pageContinuation, pageContinuation.IsValid);

            var repository = new SourceRepository(pageContinuation.Source.PackageSources.FirstOrDefault(), Repository.Provider.GetCoreV3());

            var observedProjects = _extensibleProjectLocator.GetAllExtensibleProjects();

            var httpHandler = await repository.GetResourceAsync<HttpHandlerResourceV3>();

            try
            {
                var localPackages = await _projectManager.CreatePackagesCollectionFromProjectsAsync(observedProjects, token);

                var pagedPackages = localPackages
                    .GetLatest(VersionComparer.Default)
                    .Where(package => package.Id.IndexOf(searchTerm ?? String.Empty, StringComparison.OrdinalIgnoreCase) != -1)
                    .OrderBy(package => package.Id)
                    .Skip(pageContinuation.GetNext());


                if (pageContinuation.Size > 0)
                {
                    pagedPackages = pagedPackages.Take(pageContinuation.Size).ToList();
                }

                List<IPackageSearchMetadata> combinedFindedMetadata = new List<IPackageSearchMetadata>();

                foreach (var package in pagedPackages)
                {
                    var metadata = await GetPackageMetadataAsync(package, searchFilter.IncludePrerelease, token);

                    if (metadata != null)
                    {
                        combinedFindedMetadata.Add(metadata);
                    }
                }

                return combinedFindedMetadata;
            }
            catch (FatalProtocolException ex) when (token.IsCancellationRequested)
            {
                //task is cancelled, supress
                throw new OperationCanceledException("Search request was canceled", ex, token);
            }
            finally
            {
                var credentialsService = httpHandler.GetCredentialServiceImplementation<ExplorerCredentialService>();

                if (credentialsService != null)
                {
                    credentialsService.ClearRetryCache();
                }
            }
        }

        public IPackageMetadataProvider InitializeMetdataProvider()
        {
            //todo provide more automatic way
            //create package metadata provider from context
            var context = _repositoryService.AcquireContext();

            var projects = _extensibleProjectLocator.GetAllExtensibleProjects();

            var localRepos = _projectManager.AsLocalRepositories(projects);

            var repos = context.Repositories ?? context.PackageSources.Select(src => _repositoryService.GetRepository(src));

            return new PackageMetadataProvider(repos, localRepos);
        }

        public async Task<IPackageSearchMetadata> GetPackageMetadataAsync(PackageIdentity identity, bool includePrerelease, CancellationToken cancellationToken)
        {
            // first we try and load the metadata from a local package
            var packageMetadata = await PackageMetadataProvider.Value.GetLocalPackageMetadataAsync(identity, includePrerelease, cancellationToken);

            if (packageMetadata == null)
            {
                // and failing that we go to the network
                packageMetadata = await PackageMetadataProvider.Value.GetPackageMetadataAsync(identity, includePrerelease, cancellationToken);
            }
            return packageMetadata;
        }
    }
}
