namespace NuGetPackageManager.Providers
{
    using NuGetPackageManager.Cache;

    public class ApplcationCacheProvider : IApplicationCacheProvider
    {
        IconCache iconCache;

        public IconCache EnsureIconCache()
        {
            if (iconCache == null)
            {
                iconCache = new IconCache();
            }

            return iconCache;
        }
    }
}
