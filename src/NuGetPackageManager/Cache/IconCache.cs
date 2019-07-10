namespace NuGetPackageManager.Cache
{
    using System;
    using System.IO;
    using System.Runtime.Caching;
    using System.Windows.Media.Imaging;

    public class IconCache
    {
        MemoryCache Cache { get; set; } = new MemoryCache("Icon cache");

        public IconCache(CacheItemPolicy cacheItemPolicy = null)
        {
            StoringPolicy = cacheItemPolicy ?? DefaultStoringPolicy;
        }

        public CacheItemPolicy StoringPolicy { get; private set; }

        public static CacheItemPolicy DefaultStoringPolicy = new CacheItemPolicy();

        public void SaveToCache(Uri iconUri, byte[] streamContent)
        {
            Cache.Add(iconUri.ToString(), streamContent, StoringPolicy);
        }

        public BitmapImage GetFromCache(Uri iconUri)
        {
            //todo stream should be disposed when item removed from cache
            string key = iconUri.ToString();
            var cachedItem = Cache.Get(key) as byte[];

            if (cachedItem == null)
            {
                throw new InvalidOperationException($"Key '{key}' not found in cache");
            }

            using (var stream = new MemoryStream())
            {
                var image = new BitmapImage();

                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();

                return image;
            }
        }
    }
}
