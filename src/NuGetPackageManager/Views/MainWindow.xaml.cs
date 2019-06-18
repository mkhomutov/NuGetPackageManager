using Catel.Windows.Threading;
using NuGetPackageManager.Controls.Helpers;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NuGetPackageManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Catel.Windows.DataWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            ShowInTaskbar = true;

            //var control = Application.Current.FindResource(typeof(ProgressBar));
            //var value = XamlExportHelper.Save(control);
        }
    }
}
