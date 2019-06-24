using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    /// Interaction logic for ExplorerTopBarView.xaml
    /// </summary>
    public partial class ExplorerTopBarView : Catel.Windows.Controls.UserControl
    {
        public ExplorerTopBarView()
        {
            InitializeComponent();
        }

        public DependencyObject UsedOn
        {
            get { return (DependencyObject)GetValue(UsedOnProperty); }
            set { SetValue(UsedOnProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TabSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UsedOnProperty =
            DependencyProperty.Register("UsedOn", typeof(DependencyObject), typeof(ExplorerTopBarView), new PropertyMetadata(null));
    }
}
