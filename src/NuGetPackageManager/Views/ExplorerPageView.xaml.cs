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

        [ViewToViewModel(viewModelPropertyName: "SelectedPackage", MappingType = ViewToViewModelMappingType.TwoWayViewModelWins)]
        public IPackageSearchMetadata SelectedItemOnPage
        {
            get { return (IPackageSearchMetadata)GetValue(SelectedItemOnPageProperty); }
            set { SetValue(SelectedItemOnPageProperty, value); }
        }

        public static readonly DependencyProperty SelectedItemOnPageProperty =
            DependencyProperty.Register(nameof(SelectedItemOnPage), typeof(IPackageSearchMetadata), typeof(ExplorerPageView), new PropertyMetadata(null));
    }
}
