using Catel.Windows;
using NuGetPackageManager.ViewModels;

namespace NuGetPackageManager.Views
{
    public partial class MainWindow : DataWindow
    {
        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(MainViewModel viewModel)
            : base(viewModel, DataWindowMode.Custom)
        {
            InitializeComponent();
        }

        #endregion
    }
}