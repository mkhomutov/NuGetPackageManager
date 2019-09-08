using NuGet.ProjectManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Windows.Dialogs
{
    public class FileConflictDialogOption : IDialogOption<FileConflictAction>
    {
        public FileConflictDialogOption(Func<FileConflictAction> optionCallback)
        {
            DialogCallback = optionCallback;
        }

        public bool IsOkBehavior { get; set; }

        public bool IsApplyBehavior { get; set; }

        public bool IsCancelBehavior { get; set; }

        public bool IsDefault { get; set; }

        public string Caption { get; set; }

        public Func<FileConflictAction> DialogCallback { get; private set; }

        public static IDialogOption Ignore = new FileConflictDialogOption(() => FileConflictAction.Ignore)
        {
            Caption = "Ignore",
            IsCancelBehavior = true
        };

        public static IDialogOption IgnoreAll = new FileConflictDialogOption(() => FileConflictAction.IgnoreAll)
        {
            Caption = "Ignore All",
            IsCancelBehavior = true
        };

        public static IDialogOption OverWrite = new FileConflictDialogOption(() => FileConflictAction.Overwrite)
        {
            Caption = "Overwrite",
            IsOkBehavior = true
        };

        public static IDialogOption OverWriteAll = new FileConflictDialogOption(() => FileConflictAction.OverwriteAll)
        {
            Caption = "Overwrite All",
            IsOkBehavior = true
        };
    }
}
