using Catel.Windows.Interactivity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace NuGetPackageManager.Behaviors
{
    public class SelectFirstItemOnContextBehavior : BehaviorBase<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnAssociatedObjectLoaded()
        {
            TrySelectFirstItemFromSource();
        }

        private void TrySelectFirstItemFromSource()
        {
            if(AssociatedObject.Items != null && AssociatedObject.Items.Count > 0)
            {
                AssociatedObject.SetCurrentValue(Selector.SelectedIndexProperty, 0);
            }
        }
    }
}
