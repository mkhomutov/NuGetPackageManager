namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.MVVM;
    using NuGetPackageManager.Management;
    using NuGetPackageManager.Models;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    public class ProjectsViewModel : ViewModelBase
    {
        private readonly IExtensibleProjectManager _extensiblesManager;

        public ProjectsViewModel(NuGetActionTarget projectsModel, IExtensibleProjectManager extensiblesManager)
        {
            Argument.IsNotNull(() => extensiblesManager);
            Argument.IsNotNull(() => projectsModel);

            _extensiblesManager = extensiblesManager;
            ProjectsModel = projectsModel;
        }

        [Model]
        public NuGetActionTarget ProjectsModel { get; set; }

        public ObservableCollection<CheckableUnit<IExtensibleProject>> Projects { get; set; }

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

        private void NotifyOnProjectSelectionChanged(bool isSelected, IExtensibleProject project)
        {
            if (isSelected)
            {
                ProjectsModel.Add(project);
            }
            else
            {
                ProjectsModel.Remove(project);
            }
        }
    }
}
