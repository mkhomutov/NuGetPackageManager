using Catel.Collections;
using NuGetPackageManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager
{
    public interface IManagerPage
    {
        FastObservableCollection<PackageDetailsViewModel> PackageItems { get; }

        void StartLoadingTimerOrInvalidateData();
    }
}
