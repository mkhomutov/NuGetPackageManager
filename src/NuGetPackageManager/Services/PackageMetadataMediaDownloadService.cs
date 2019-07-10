namespace NuGetPackageManager.Services
{
    using Catel;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Cache;
    using NuGetPackageManager.Providers;
    using NuGetPackageManager.Web;
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class PackageMetadataMediaDownloadService : IPackageMetadataMediaDownloadService
    {
        private static readonly IconDownloader iconDownloader = new IconDownloader();

        private readonly IconCache iconCache;

        public PackageMetadataMediaDownloadService(IApplicationCacheProvider appCacheProvider)
        {
            Argument.IsNotNull(() => appCacheProvider);

            iconCache = appCacheProvider.EnsureIconCache();
        }

        public async Task DownloadFromAsync(IPackageSearchMetadata packageMetadata)
        {
            try
            {
                if (packageMetadata.IconUrl == null)
                {
                    throw new ArgumentException("Download source is null");
                }

                using (var ms = new MemoryStream())
                {
                    using (var contentStream = await iconDownloader.GetByUrlAsync(packageMetadata.IconUrl))
                    {
                        contentStream.CopyTo(ms);
                        var bytes = ms.ToArray();
                        //store to cache
                        iconCache.SaveToCache(packageMetadata.IconUrl, bytes);
                    }
                }
            }
            catch (Exception e)
            {
                var b = e;
                throw;
            }
        }
    }
}
