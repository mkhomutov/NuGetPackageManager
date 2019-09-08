using Catel.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Windows
{
    public interface IDialogViewModel
    {
        DialogCustomization Dialog { get; }
        TaskCommand<IDialogOption> RunOption { get; }
    }
}
