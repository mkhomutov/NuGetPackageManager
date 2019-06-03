using Catel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Providers
{
    public class ModelProvider<T> : IModelProvider<T> where T : ModelBase
    {
        private T _model;

        public T Model {
            get => _model;
            set {
                if(value != _model)
                {
                    _model = value;
                    RaisePropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged()
        {
             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
        }
    }
}
