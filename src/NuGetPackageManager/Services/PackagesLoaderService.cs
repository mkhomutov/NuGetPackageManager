namespace NuGetPackageManager.Services
{
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Pagination;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class PackagesLoaderService : IPackagesLoaderService
    {
        public async Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token)
        {
            var repository = new SourceRepository(pageContinuation.Source.SingleOrDefault(), Repository.Provider.GetCoreV3());

            var searchResource = await repository.GetResourceAsync<PackageSearchResource>();

            try
            {
                var packages = await searchResource.SearchAsync(searchTerm, searchFilter, pageContinuation.GetNext(), pageContinuation.Size, new Loggers.DebugLogger(true), token);

                return packages;
            }
            catch (FatalProtocolException ex) when (token.IsCancellationRequested)
            {
                //task is cancelled, supress
                throw new OperationCanceledException("Search request was cancelled", ex, token);
            }
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> LoadAsyncFromSources(string searchTerm, PageContinuation pageContinuation,
            SearchFilter searchFilter, CancellationToken token)
        {
            var searchResource = await MultiplySourceSearchResource.CreateAsync(
                pageContinuation.Source.Select(s => new SourceRepository(s, Repository.Provider.GetCoreV3())).ToArray());

            try
            {
                var packages = await searchResource.SearchAsync(searchTerm, searchFilter, pageContinuation.GetNext(), pageContinuation.Size, new Loggers.DebugLogger(true), token);

                return packages;
            }
            catch (FatalProtocolException ex) when (token.IsCancellationRequested)
            {
                //task is cancelled, supress
                throw new OperationCanceledException("Search request was cancelled", ex, token);
            }
        }
    }
}
