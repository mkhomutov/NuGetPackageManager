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
    public interface IPackagesLoaderService
    {
        Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token);
    }
}
