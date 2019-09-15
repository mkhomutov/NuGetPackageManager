namespace NuGetPackageManager.Windows
{
    using Catel.MVVM;

    public interface IProgressManager
    {
        void ShowBar(IViewModel vm);

        void HideBar(IViewModel vm);
    }
}
