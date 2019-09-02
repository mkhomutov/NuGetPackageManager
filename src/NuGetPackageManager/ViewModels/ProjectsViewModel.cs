namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.MVVM;
    using NuGetPackageManager.Management;
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    public class ProjectsViewModel : ViewModelBase
    {
        private readonly IExtensibleProjectManager _extensiblesManager;

        public ProjectsViewModel(IExtensibleProjectManager extensiblesManager)
        {
            Argument.IsNotNull(() => extensiblesManager);

            _extensiblesManager = extensiblesManager;
        }

        protected override Task InitializeAsync()
        {
            if (!_extensiblesManager.IsConfigLoaded)
            {
                _extensiblesManager.RestoreStateFromConfig();
            }

            var availableProjects = _extensiblesManager.GetAllExtensibleProjects()
                .Where(x => _extensiblesManager.IsEnabled(x));

            Projects = new ObservableCollection<CheckableUnit<IExtensibleProject>>(availableProjects
                .Select(x =>
                    new CheckableUnit<IExtensibleProject>(false, x, NotifyOnProjectSelectionChanged)));

            return base.InitializeAsync();
        }

        public ObservableCollection<CheckableUnit<IExtensibleProject>> Projects { get; set; }

        private void NotifyOnProjectSelectionChanged(bool isSelected, IExtensibleProject project)
        {
            //todo track selection to services/provider for further actions
            throw new NotImplementedException();
        }
    }
}
