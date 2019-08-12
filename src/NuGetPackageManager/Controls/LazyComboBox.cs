using Catel.MVVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NuGetPackageManager.Controls
{
    public class LazyComboBox : ComboBox
    {
        protected override void OnDropDownOpened(EventArgs e)
        {
            ExecuteItemSourceInitializationCommand();
            base.OnDropDownOpened(e);
        }

        public TaskCommand Command
        {
            get { return (TaskCommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(TaskCommand), typeof(LazyComboBox), new PropertyMetadata(null));

        protected void ExecuteItemSourceInitializationCommand()
        {
            Command?.Execute();
        }
    }
}
