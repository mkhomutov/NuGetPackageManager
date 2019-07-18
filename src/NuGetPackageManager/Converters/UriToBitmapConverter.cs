namespace NuGetPackageManager.Converters
{
    using Catel.IoC;
    using Catel.MVVM.Converters;
    using NuGetPackageManager.Cache;
    using NuGetPackageManager.Providers;
    using System;
    using System.Globalization;
    using System.Windows;

    public class UriToBitmapConverter : IValueConverter
    {
        private static readonly IconCache iconCache;

        static UriToBitmapConverter()
        {
            if (iconCache == null)
            {
                var appCacheProvider = ServiceLocator.Default.ResolveType<IApplicationCacheProvider>();

                iconCache = appCacheProvider.EnsureIconCache();
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var uri = value != null ? (Uri)value : null;

            //get bitmap from stream cache
            try
            {
                return iconCache.GetFromCache(uri);
            }
            catch (Exception e)
            {
                var b = e;
                return DependencyProperty.UnsetValue;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
