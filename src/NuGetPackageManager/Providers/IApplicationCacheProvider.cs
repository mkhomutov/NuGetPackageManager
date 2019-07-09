using NuGetPackageManager.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Providers
{
    public interface IApplicationCacheProvider
    {
        IconCache EnsureIconCache();
    }
}
