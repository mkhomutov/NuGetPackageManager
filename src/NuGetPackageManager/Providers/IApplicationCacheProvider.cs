namespace NuGetPackageManager.Providers
{
    using NuGetPackageManager.Cache;

    public interface IApplicationCacheProvider
    {
        IconCache EnsureIconCache();
    }
}
