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
using System.Windows.Shapes;

namespace NuGetPackageManager.Views
{
    /// <summary>
    /// Interaction logic for ExtensiblesWindow.xaml
    /// </summary>
    public partial class ExtensiblesWindow : Catel.Windows.DataWindow
    {
        public ExtensiblesWindow() : base(Catel.Windows.DataWindowMode.Close)
        {
            InitializeComponent();
        }
    }
}
