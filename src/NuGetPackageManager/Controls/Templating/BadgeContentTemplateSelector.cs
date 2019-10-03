using NuGetPackageManager.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NuGetPackageManager.Controls.Templating
{
    public class BadgeContentTemplateSelector : DataTemplateSelector
    {
        public DataTemplate NotAvailable { get; set; }
        public DataTemplate Available { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if(item != null)
            {
                var state = (PackageStatus)item;

                if(state == PackageStatus.LastVersionInstalled)
                {
                    return NotAvailable;
                }
                if(state == PackageStatus.UpdateAvailable)
                {
                    return Available;
                }
            }
            return base.SelectTemplate(item, container);
        }
    }
}
