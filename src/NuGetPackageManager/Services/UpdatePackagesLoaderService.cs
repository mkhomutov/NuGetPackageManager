namespace NuGetPackageManager.Services
{
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Pagination;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class UpdatePackagesLoaderService : IPackagesLoaderService
    {
        public Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
