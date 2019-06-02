using Catel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetPackageManager.Xaml.Providers
{
    public interface IModelProvider<T> : INotifyPropertyChanged where T: ModelBase 
    {
        T Model { get; set; }
    }
}
