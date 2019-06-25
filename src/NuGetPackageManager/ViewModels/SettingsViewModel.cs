namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.Fody;
    using Catel.MVVM;
    using NuGetPackageManager.Models;
    using System.Collections.ObjectModel;

    public class SettingsViewModel : ViewModelBase
    {
        public SettingsViewModel(ExplorerSettingsContainer settings)
        {
            Argument.IsNotNull(() => settings);
            Settings = settings;
        }

        [Model]
        [Expose("NuGetFeeds")]
        public ExplorerSettingsContainer Settings { get; set; }

    }
}
