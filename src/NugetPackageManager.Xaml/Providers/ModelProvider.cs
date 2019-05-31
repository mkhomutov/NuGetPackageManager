using Catel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetPackageManager.Xaml.Providers
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
                    OnModelChanged();
                }
            }
        }

        public event PropertyChangedEventHandler ModelChanged;

        private void OnModelChanged()
        {
            if(ModelChanged != null)
            {
                ModelChanged.Invoke(this, new PropertyChangedEventArgs(nameof(Model)));
            }
        }
    }
}
