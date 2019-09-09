using Catel.MVVM;
using Catel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Windows.Service
{
    public interface ISynchronousUiVisualizer
    {
        bool? ShowDialog(IViewModel viewModel, EventHandler<UICompletedEventArgs> completedProc = null);
    }
}
