using Catel.MVVM.Converters;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace NuGetPackageManager.Converters
{
    [ValueConversion(typeof(ICollection), typeof(Visibility))]
    public class EmptyCollectionToVisibleConverter : Catel.MVVM.Converters.IValueConverter
    {
        private static readonly CollectionToCollapsingVisibilityConverter _collectionToVisibility = new CollectionToCollapsingVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                var visibility = (Visibility)_collectionToVisibility.Convert(value, targetType, parameter, culture);

                return visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            }
            catch(Exception)
            {
                throw;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
