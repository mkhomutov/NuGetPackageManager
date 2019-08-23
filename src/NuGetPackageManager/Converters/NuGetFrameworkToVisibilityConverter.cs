namespace NuGetPackageManager.Converters
{
    using Catel.MVVM.Converters;
    using NuGet.Frameworks;
    using System;
    using System.Globalization;
    using System.Windows;

    public class NuGetFrameworkToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var framework = value as NuGetFramework;
            if (framework == null || framework.IsAny)
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
