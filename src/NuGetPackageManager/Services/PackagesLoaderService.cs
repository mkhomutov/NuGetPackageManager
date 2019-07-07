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
        public async Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token)
        {
            var repository = new SourceRepository(pageContinuation.Source, Repository.Provider.GetCoreV3());

            var searchResource = await repository.GetResourceAsync<PackageSearchResource>();

            var packages = await searchResource.SearchAsync(String.Empty, searchFilter, pageContinuation.GetNext(), pageContinuation.Size, new Loggers.DebugLogger(true), token);

            return packages;
        }
    }
}
