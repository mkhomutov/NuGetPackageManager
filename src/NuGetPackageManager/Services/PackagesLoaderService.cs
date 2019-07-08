using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public class PackagesLoaderService : IPackagesLoaderService
    {
        public async Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token)
        {
            var repository = new SourceRepository(pageContinuation.Source, Repository.Provider.GetCoreV3());

            var searchResource = await repository.GetResourceAsync<PackageSearchResource>();

            try
            {
                var packages = await searchResource.SearchAsync(searchTerm, searchFilter, pageContinuation.GetNext(), pageContinuation.Size, new Loggers.DebugLogger(true), token);

                return packages;
            }
            catch(FatalProtocolException ex) when (token.IsCancellationRequested)
            {
                //task is cancelled, supress
                throw new OperationCanceledException("Search request was cancelled", ex, token);
            }
        }
    }
}
