namespace NuGetPackageManager
{
    using Catel.Collections;
    using NuGetPackageManager.ViewModels;

    public interface IManagerPage
    {
        FastObservableCollection<PackageDetailsViewModel> PackageItems { get; }

        void StartLoadingTimerOrInvalidateData();
    }
}
