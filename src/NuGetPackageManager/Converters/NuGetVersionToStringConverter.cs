using Catel.MVVM.Converters;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Converters
{
    public class NuGetVersionToStringConverter : ValueConverterBase<NuGetVersion, string>
    {
        protected override object Convert(NuGetVersion value, Type targetType, object parameter)
        {
            return value?.ToString() ?? Constants.NotInstalled;
        }
    }
}
