using NuGetPackageManager.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NuGetPackageManager.Views
{
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
