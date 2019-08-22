using Catel.MVVM.Converters;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Converters
{
    public class NuGetFrameworkToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var framework = value as NuGetFramework;
            if (framework == null)
            {
                return string.Empty;
            }

            return framework.IsSpecificFramework ? framework.ToString() : framework.Framework;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
