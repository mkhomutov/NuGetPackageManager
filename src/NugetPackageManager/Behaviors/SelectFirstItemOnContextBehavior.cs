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
    public class SelectFirstItemOnContextBehavior : BehaviorBase<ListView>
    {
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            base.OnAttached();
        }

        private void AssociatedObject_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            TrySelectFirstItemFromSource();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            base.OnDetaching();
        }

        private void OnDataContextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
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
