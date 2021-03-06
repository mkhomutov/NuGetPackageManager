﻿namespace NuGetPackageManager.Converters
{
    using Catel.MVVM.Converters;
    using NuGet.Frameworks;
    using System;

    public class NuGetFrameworkToStringConverter : ValueConverterBase<NuGetFramework, string>
    {
        protected override object Convert(NuGetFramework value, Type targetType, object parameter)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return value.IsSpecificFramework ? value.ToString() : value.Framework;
        }
    }
}
