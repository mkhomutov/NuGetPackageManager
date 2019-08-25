namespace NuGetPackageManager.Converters
{
    using Catel;
    using Catel.MVVM.Converters;
    using System;
    using System.Globalization;

    public class BoolToIntConverter : ValueConverterBase<bool, int>
    {
        protected override object Convert(bool value, Type targetType, object parameter)
        {
            return value ? 1 : 0;
        }
    }
}
