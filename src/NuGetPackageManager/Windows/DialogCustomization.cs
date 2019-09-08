using Catel.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Windows
{
    public class DialogCustomization
    {
        public DialogCustomization(IEnumerable<IDialogOption> options, bool isCloseButtonAvailable)
        {
            Options = options;
            IsCloseButtonAvaialble = isCloseButtonAvailable;
        }

        public IEnumerable<IDialogOption> Options { get; }

        public bool IsCloseButtonAvaialble { get; }
    }
}
