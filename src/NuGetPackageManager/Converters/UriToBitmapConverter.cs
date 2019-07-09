using Catel.IoC;
using Catel.MVVM.Converters;
using NuGetPackageManager.Cache;
using NuGetPackageManager.Providers;
using NuGetPackageManager.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace NuGetPackageManager.Converters
{
    public class UriToBitmapConverter : IValueConverter
    {
        private static readonly IconCache iconCache;

        static UriToBitmapConverter()
        {
            if(iconCache == null)
            {
                var appCacheProvider = ServiceLocator.Default.ResolveType<IApplicationCacheProvider>();

                iconCache = appCacheProvider.EnsureIconCache();
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is null)
            {
                return DependencyProperty.UnsetValue;
            }
            if(value is Uri)
            {
                var uri = (Uri)value;

                //get bitmap from stream cache
                try
                {
                    return iconCache.GetFromCache(uri);
                }
                catch(Exception e)
                {
                    var b = e;
                    return DependencyProperty.UnsetValue;
                }
            }
            else
            {
                throw new ArgumentException($"Converter used on value '{value}' of unsupported type");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
