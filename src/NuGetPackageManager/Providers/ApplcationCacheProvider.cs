using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NuGetPackageManager.Cache;

namespace NuGetPackageManager.Providers
{
    public class ApplcationCacheProvider : IApplicationCacheProvider
    {
        IconCache _iconCache;

        public IconCache EnsureIconCache()
        {
            if (_iconCache == null)
            {
                _iconCache = new IconCache();
            }

            return _iconCache;
        }
    }
}
