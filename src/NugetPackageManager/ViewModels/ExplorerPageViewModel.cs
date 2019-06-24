using Catel.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.ViewModels
{
    public class ExplorerPageViewModel : ViewModelBase
    {

        public ExplorerPageViewModel(string pageTitle)
        {
            Title = pageTitle;
        }
    }
}
