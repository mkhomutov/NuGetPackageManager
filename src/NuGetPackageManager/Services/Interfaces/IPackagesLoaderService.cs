namespace NuGetPackageManager.Services
{
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Pagination;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public interface IPackagesLoaderService
    {
        Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token);
    }
}
