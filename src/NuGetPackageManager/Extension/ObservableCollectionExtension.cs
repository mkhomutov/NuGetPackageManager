using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Extension
{
    public static class ObservableCollectionExtension
    {
        public static void MoveUp<T>(this ObservableCollection<T> collection, T item)
        {
            var oldindex = collection.IndexOf(item);

            if(oldindex == collection.Count-1)
            {
                return;
            }

            collection.Move(oldindex, oldindex + 1);
        }

        public static void MoveDown<T>(this ObservableCollection<T> collection, T item)
        {
            var oldindex = collection.IndexOf(item);
            if (oldindex == 0)
            {
                return;
            }

            collection.Move(oldindex, oldindex - 1);
        }
    }
}
