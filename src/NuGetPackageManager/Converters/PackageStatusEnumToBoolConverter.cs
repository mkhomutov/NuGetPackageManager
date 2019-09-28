namespace NuGetPackageManager.Converters
{
    using Catel.MVVM.Converters;
    using NuGetPackageManager.Enums;
    using System;

    public class PackageStatusEnumToBoolConverter : ValueConverterBase<PackageStatus, bool>
    {
        protected override object Convert(PackageStatus value, Type targetType, object parameter)
        {
            return Math.Abs((int)value) <= 1;
        }
    }
}
