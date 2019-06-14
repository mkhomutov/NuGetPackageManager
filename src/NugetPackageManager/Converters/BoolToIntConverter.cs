namespace NuGetPackageManager.Converters
{
    using Catel;
    using Catel.MVVM.Converters;
    using System;
    using System.Globalization;

    public class BoolToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Argument.IsNotNull(() => value);

            bool from = (bool)value;

            return from ? 1 : 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
