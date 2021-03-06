namespace NuGetPackageManager.Views
{
    using NuGetPackageManager.ViewModels;

    /// <summary>
    /// Логика взаимодействия для SettingsControlView.xaml
    /// </summary>
    public partial class SettingsControlView : Catel.Windows.Controls.UserControl
    {
        public SettingsControlView()
        {
            InitializeComponent();
        }

        public SettingsControlView(SettingsViewModel viewModel) : base(viewModel)
        {
            InitializeComponent();
        }
    }
}
