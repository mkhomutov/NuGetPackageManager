using Catel;
using Catel.Configuration;
using Catel.Logging;
using Catel.MVVM;
using NuGetPackageManager.Models;
using NuGetPackageManager.Providers;
using NuGetPackageManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.ViewModels
{
    public class SettingsControlViewModel : ViewModelBase
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();
        private readonly NugetConfigurationService _configurationService;
        private readonly IModelProvider<NuGetFeed> _modelProvider;

        public SettingsControlViewModel(IConfigurationService configurationService, IModelProvider<NuGetFeed> modelProvider)
        {
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => modelProvider);

            _configurationService = configurationService as NugetConfigurationService;
            _modelProvider = modelProvider;
            CommandInitialize();
            Title = "Settings";
        }


        public ObservableCollection<NuGetFeed> Feeds { get; set; } = new ObservableCollection<NuGetFeed>();

        [Model]
        public NuGetFeed SelectedFeed { get; set; }

        #region commands

        public Command RemoveFeed { get; set; }

        private void OnRemoveFeedExecute()
        {
            Feeds.Remove(SelectedFeed);
        }

        public Command MoveUpFeed { get; set; }

        private void OnMoveUpFeedExecute()
        {
            Feeds.MoveUp(SelectedFeed);
        }

        public Command MoveDownFeed { get; set; }

        private void OnMoveDownFeedExecute()
        {
            Feeds.MoveDown(SelectedFeed);
        }

        public Command AddFeed { get; set; }

        private void OnAddFeedExecute()
        {
            Feeds.Add(new NuGetFeed(Constants.NamePlaceholder, Constants.SourcePlaceholder));
        }

        #endregion

        protected void CommandInitialize()
        {
            RemoveFeed = new Command(OnRemoveFeedExecute);
            MoveUpFeed = new Command(OnMoveUpFeedExecute);
            MoveDownFeed = new Command(OnMoveDownFeedExecute);
            AddFeed = new Command(OnAddFeedExecute);
        }

        protected override Task InitializeAsync()
        {
            if (_configurationService.IsValueAvailable(ConfigurationContainer.Local, $"feed{0}"))
            {
                ReadFeedsFromConfiguration();
            }
            else
            {
                AddDefaultFeeds();
            }

            //handle manual model save on child viewmodel
            _modelProvider.PropertyChanged += OnModelProviderModelChanged;

            return base.InitializeAsync();
        }

        protected override Task<bool> SaveAsync()
        {
            //store all feed inside configuration
            for (int i = 0; i<Feeds.Count; i++)
            {
                _configurationService.SetValue(ConfigurationContainer.Local, $"feed{i}", Feeds[i]);
            }

            return base.SaveAsync();
        }

        protected override Task CloseAsync()
        {
            _modelProvider.PropertyChanged -= OnModelProviderModelChanged;
            return base.CloseAsync();
        }

        private void ReadFeedsFromConfiguration()
        {
            NuGetFeed temp = null; ;
            int i = 0;

            //restore values from configuration
            while(_configurationService.IsLocalValueAvailable($"feed{i}"))
            {
                temp = _configurationService.GetValue(ConfigurationContainer.Local, $"feed{i}");

                if (temp != null)
                {
                    Feeds.Add(temp);
                }
                else
                {
                    _log.Error($"Configuration value under key {i} is broken and cannot be loaded");
                }

                i++;
            }
        }

        private void OnModelProviderModelChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //should drop current selected row and add updated
            Feeds.Remove(SelectedFeed);
            Feeds.Add(_modelProvider.Model);
        }

        private void AddDefaultFeeds()
        {
            Feeds.Add(
                new NuGetFeed(
                Constants.DefaultNugetOrgName, 
                Constants.DefaultNugetOrgUri)
                );
        }
    }
}
