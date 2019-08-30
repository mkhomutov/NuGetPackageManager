namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.IoC;
    using Catel.MVVM;
    using Catel.Services;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Models;
    using NuGetPackageManager.Providers;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class MainViewModel : ViewModelBase
    {
        private readonly IUIVisualizerService _uIVisualizerService;

        private readonly ITypeFactory _typeFactory;


        public MainViewModel(ITypeFactory typeFactory, IUIVisualizerService service, ICommandManager commandManager, IModelProvider<ExplorerSettingsContainer> settingsProvider)
        {
            Argument.IsNotNull(() => service);
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => commandManager);
            Argument.IsNotNull(() => settingsProvider);

            _uIVisualizerService = service;
            _typeFactory = typeFactory;

            CreateApplicationWideCommands(commandManager);

            Settings = settingsProvider.Model;
        }

        protected override Task InitializeAsync()
        {
            ExplorerPages = new ObservableCollection<ExplorerPageViewModel>();

            CreatePages();
            return base.InitializeAsync();
        }

        public ExplorerSettingsContainer Settings { get; set; }

        public IPackageSearchMetadata SelectedPackageMetadata { get; set; }

        public ObservableCollection<ExplorerPageViewModel> ExplorerPages { get; set; }

        private void CreatePages()
        {
            string[] pageNames = new string[] { "Browse", "Installed", "Updates" };

            for (int i = 0; i < pageNames.Length; i++)
            {
                var newPage = CreatePage(pageNames[i]);

                if (newPage != null)
                {
                    ExplorerPages.Add(newPage);
                }
            }

            ExplorerPageViewModel CreatePage(string title)
            {
                return _typeFactory.CreateInstanceWithParametersAndAutoCompletion<ExplorerPageViewModel>(Settings, title);
            }
        }

        private void CreateApplicationWideCommands(ICommandManager cm)
        {
            //move to initializer
            cm.CreateCommand("RefreshCurrentPage", new Catel.Windows.Input.InputGesture(Key.F5));
        }
    }
}
