using NuGet.Protocol.Core.Types;

namespace NuGetPackageManager.Cache
{
    public interface INuGetCacheManager
    {
        bool ClearAll();

        SourceCacheContext GetCacheContext();

        HttpSourceCacheContext GetHttpCacheContext();

        HttpSourceCacheContext GetHttpCacheContext(int retryCount, bool directDownload = false);
    }
}
