namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.IoC;
    using Catel.MVVM;
    using Catel.Services;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Models;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;

    public class MainViewModel : ViewModelBase
    {
        private readonly IUIVisualizerService _uIVisualizerService;

        private readonly ITypeFactory _typeFactory;

        public MainViewModel(ITypeFactory typeFactory, IUIVisualizerService service)
        {
            Argument.IsNotNull(() => service);
            Argument.IsNotNull(() => typeFactory);

            _uIVisualizerService = service;
            _typeFactory = typeFactory;
        }

        protected override Task InitializeAsync()
        {
            ExplorerPages = new ObservableCollection<ExplorerPageViewModel>();
            
            CreatePages();
            return base.InitializeAsync();
        }

        public ExplorerSettingsContainer Settings { get; set; } = new ExplorerSettingsContainer();

        public IPackageSearchMetadata SelectedPackageMetadata { get; set; } 

        public ObservableCollection<ExplorerPageViewModel> ExplorerPages { get; set; }

        private void CreatePages()
        {
            string[] pageNames = new string[] { "Browse", "Installed", "Updates", "Consolidate" };

            for (int i = 0; i < 4; i++)
            {
                var newPage = CreatePage(pageNames[i]);

                if (newPage != null)
                {
                    ExplorerPages.Add(newPage);
                }
            }

            ExplorerPageViewModel CreatePage(string title)
            {
                return _typeFactory.CreateInstanceWithParametersAndAutoCompletion<ExplorerPageViewModel>(Settings,title);
            }
        }
    }
}
