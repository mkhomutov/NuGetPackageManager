using Catel.MVVM.Views;
using NuGetPackageManager.Models;
using System.Windows;

namespace NuGetPackageManager.Views
{
    /// <summary>
    /// Interaction logic for ExplorerPageView.xaml
    /// </summary>
    public partial class ExplorerPageView : Catel.Windows.Controls.UserControl
    {
        public ExplorerPageView()
        {
            InitializeComponent();
        }


        //[ViewToViewModel(MappingType = ViewToViewModelMappingType.ViewToViewModel)]
        //public ExplorerSettingsContainer Settings
        //{
        //    get { return (ExplorerSettingsContainer)GetValue(SettingsProperty); }
        //    set { SetValue(SettingsProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty SettingsProperty =
        //    DependencyProperty.Register(nameof(Settings), typeof(ExplorerSettingsContainer), typeof(ExplorerPageView), new PropertyMetadata(null));
    }
}
