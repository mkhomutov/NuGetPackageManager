namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.Configuration;
    using Catel.IoC;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Services;
    using NuGetPackageManager.Models;
    using NuGetPackageManager.Services;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;

    public class ExplorerTopBarViewModel : ViewModelBase
    {
        private readonly ITypeFactory _typeFactory;

        private readonly IUIVisualizerService _uIVisualizerService;

        private readonly NugetConfigurationService _configurationService;

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public ExplorerTopBarViewModel(ExplorerSettingsContainer settings, ITypeFactory typeFactory, IUIVisualizerService uIVisualizerService, IConfigurationService configurationService)
        {
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => uIVisualizerService);
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => settings);

            _typeFactory = typeFactory;
            _uIVisualizerService = uIVisualizerService;
            _configurationService = configurationService as NugetConfigurationService;

            Settings = settings;

            Title = "Manage Packages";
            CommandInitialize();
        }

        [Model]
        public ExplorerSettingsContainer Settings { get; set; }

        [ViewModelToModel]
        public bool IsPreReleaseIncluded { get; set; }

        [ViewModelToModel]
        public string SearchString { get; set; }

        [ViewModelToModel]
        public INuGetSource ObservedFeed { get; set; }

        public bool SelectFirstPageOnLoad { get; set; } = true;

        public ObservableCollection<INuGetSource> ActiveFeeds { get; set; }

        protected override Task InitializeAsync()
        {
            ReadFeedsFromConfiguration(Settings);

            //Log.Info("No feeds stored in configuration");
            //AddDefaultFeeds(Settings);

            ActiveFeeds = new ObservableCollection<INuGetSource>(GetActiveFeedsFromSettings());

            //select top feed
            ObservedFeed = ActiveFeeds.FirstOrDefault();

            return base.InitializeAsync();
        }

        protected void CommandInitialize()
        {
            ShowPackageSourceSettings = new TaskCommand(OnShowPackageSourceSettingsExecuteAsync);
        }

        public TaskCommand ShowPackageSourceSettings { get; set; }

        private async Task OnShowPackageSourceSettingsExecuteAsync()
        {
            var nugetSettingsVm = _typeFactory.CreateInstanceWithParametersAndAutoCompletion<SettingsViewModel>(Settings);

            if (nugetSettingsVm != null)
            {
                var result = await _uIVisualizerService.ShowDialogAsync(nugetSettingsVm);

                if (result ?? false)
                {
                    //update available feeds
                    ActiveFeeds = new ObservableCollection<INuGetSource>(GetActiveFeedsFromSettings());
                }
            }
        }

        private void ReadFeedsFromConfiguration(ExplorerSettingsContainer settings)
        {
            NuGetFeed temp = null; ;

            var keyCollection = _configurationService.GetAllKeys(ConfigurationContainer.Roaming);

            for (int i = 0; i< keyCollection.Count; i++)
            {
                temp = _configurationService.GetRoamingValue(keyCollection[i]);

                if (temp != null)
                {
                    settings.NuGetFeeds.Add(temp);
                }
                else
                {
                    Log.Error($"Configuration value under key {i} is broken and cannot be loaded");
                }
            }
        }

        private void AddDefaultFeeds(ExplorerSettingsContainer settings)
        {
            settings.NuGetFeeds.Add(
                new NuGetFeed(
                  Constants.DefaultNugetOrgName,
                  Constants.DefaultNugetOrgUri
              ));
        }

        private IEnumerable<INuGetSource> GetActiveFeedsFromSettings()
        {
            var activefeeds = Settings.NuGetFeeds.Where(x => x.IsActive).ToList<INuGetSource>();
            var allInOneSource = new CombinedNuGetSource(activefeeds);

            activefeeds.Insert(0, allInOneSource);

            return activefeeds;
        }
    }
}
