using Catel.MVVM;
using Catel.MVVM.Views;
using NuGet.Protocol.Core.Types;
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
            //prevent closing view models
            InitializeComponent();
        }

        [ViewToViewModel(viewModelPropertyName: "SelectedPackageItem", MappingType = ViewToViewModelMappingType.TwoWayViewModelWins)]
        public IViewModel SelectedItemOnPage
        {
            get { return (IViewModel)GetValue(SelectedItemOnPageProperty); }
            set { SetValue(SelectedItemOnPageProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemOnPageProperty =
            DependencyProperty.Register(nameof(SelectedItemOnPage), typeof(IViewModel), typeof(ExplorerPageView), new PropertyMetadata(null));


    }
}
