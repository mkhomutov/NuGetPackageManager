using NuGetPackageManager.Controls.Helpers;
using System.Windows;
using System.Windows.Controls;

namespace NuGetPackageManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Catel.Windows.DataWindow
    {
        public MainWindow() : base(Catel.Windows.DataWindowMode.Custom)
        {
            InitializeComponent();
            ShowInTaskbar = true;

            var control = Application.Current.FindResource(typeof(RadioButton));
            var value = XamlExportHelper.Save(control);
        }
    }
}
