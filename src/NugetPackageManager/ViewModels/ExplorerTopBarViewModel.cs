﻿namespace NuGetPackageManager.ViewModels
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

        private readonly ILog _log = LogManager.GetCurrentClassLogger();

        public ExplorerTopBarViewModel(ITypeFactory typeFactory, IUIVisualizerService uIVisualizerService, IConfigurationService configurationService)
        {
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => uIVisualizerService);
            Argument.IsNotNull(() => configurationService);

            _typeFactory = typeFactory;
            _uIVisualizerService = uIVisualizerService;
            _configurationService = configurationService as NugetConfigurationService;

            Title = "Manage Packages";
            CommandInitialize();
        }

        [Model]
        public ExplorerSettingsContainer Settings { get; set; }

        [ViewModelToModel]
        public bool IsPreReleaseIncluded { get; set; }

        public ObservableCollection<NuGetFeed> ActiveFeeds { get; set; }

        protected override Task InitializeAsync()
        {
            //todo save other flags, as using prereleases
            Settings = new ExplorerSettingsContainer();

            if (_configurationService.IsValueAvailable(ConfigurationContainer.Local, $"feed{0}"))
            {
                ReadFeedsFromConfiguration(Settings);
            }
            else
            {
                AddDefaultFeeds(Settings);
            }

            ActiveFeeds = new ObservableCollection<NuGetFeed>(GetActiveFeedsFromSettings());

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
                    ActiveFeeds = new ObservableCollection<NuGetFeed>(GetActiveFeedsFromSettings());
                }
            }
        }

        /*placeholder, this probably should be application command inside separate Catel command container*/
        public Command RefreshCurrentPage { get; set; }

        private void ReadFeedsFromConfiguration(ExplorerSettingsContainer settings)
        {
            NuGetFeed temp = null; ;
            int i = 0;

            //restore values from configuration
            while (_configurationService.IsLocalValueAvailable($"feed{i}"))
            {
                temp = _configurationService.GetValue(ConfigurationContainer.Local, $"feed{i}");

                if (temp != null)
                {
                    settings.NuGetFeeds.Add(temp);
                }
                else
                {
                    _log.Error($"Configuration value under key {i} is broken and cannot be loaded");
                }

                i++;
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

        private IEnumerable<NuGetFeed> GetActiveFeedsFromSettings()
        {
            return Settings.NuGetFeeds.Where(x => x.IsActive);
        }
    }
}