using NuGetPackageManager.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface IDefferedPackageLoaderService
    {
        void Add(DeferToken token);
        Task StartLoadingAsync();
    }
}
