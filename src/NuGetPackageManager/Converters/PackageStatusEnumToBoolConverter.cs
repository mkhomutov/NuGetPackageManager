using Catel.MVVM.Converters;
using NuGetPackageManager.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Converters
{
    public class PackageStatusEnumToBoolConverter : ValueConverterBase<PackageStatus, bool>
    {
        protected override object Convert(PackageStatus value, Type targetType, object parameter)
        {
            return Math.Abs((int)value) <= 1;
        }
    }
}
