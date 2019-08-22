namespace NuGetPackageManager.ViewModels
{
    using Catel.Data;
    using Catel.MVVM;
    using NuGet.Packaging;
    using NuGet.Protocol.Core.Types;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public class DependenciesViewModel : ViewModelBase
    {
        /// <summary>
        /// This is property inside child viewmodel mapped via attribute
        /// </summary>
        public object Collection { get; set; }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if(string.Equals(e.PropertyName, nameof(Collection)))
            {
                HasDependency = ((Collection as List<PackageDependencyGroup>)?.Count ?? 0) > 0;
            }
        }

        bool HasDependency { get; set; }


        protected override Task OnClosingAsync()
        {
            return base.OnClosingAsync();
        }
    }
}
