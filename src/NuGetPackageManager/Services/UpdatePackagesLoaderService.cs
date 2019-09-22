using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Pagination;

namespace NuGetPackageManager.Services
{
    public class UpdatePackagesLoaderService : IPackagesLoaderService
    {
        public Task<IEnumerable<IPackageSearchMetadata>> LoadAsync(string searchTerm, PageContinuation pageContinuation, SearchFilter searchFilter, CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}
