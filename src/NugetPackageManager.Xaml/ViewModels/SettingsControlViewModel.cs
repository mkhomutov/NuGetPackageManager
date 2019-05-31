using Catel.Configuration;
using Catel.Logging;
using Catel.MVVM;
using NugetPackageManager.Xaml.Services;
using NuGetPackageManager.Extension;
using NuGetPackageManager.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetPackageManager.Xaml.ViewModels
{
    public class SettingsControlViewModel : ViewModelBase
    {
        public SettingsControlViewModel(IConfigurationService configurationService)
        {
            this.configurationService = configurationService as NugetConfigurationService;
            CommandInitialize();
            Title = "Settings";
        }

        #region commands

        public Command RemoveFeed { get; set; }

        public Command MoveUpFeed { get; set; }

        public Command MoveDownFeed { get; set; }

        public Command AddFeed { get; set; }

        #endregion

        public ObservableCollection<NugetFeed> Feeds { get; set; } = new ObservableCollection<NugetFeed>();

        [Model]
        public NugetFeed SelectedFeed { get; set; }

        protected void CommandInitialize()
        {
            RemoveFeed = new Command(OnRemoveFeed);
            MoveUpFeed = new Command(OnMoveUpFeed);
            MoveDownFeed = new Command(OnMoveDownFeed);
            AddFeed = new Command(OnAddFeed);
        }

        protected override Task InitializeAsync()
        {
            if(configurationService.IsValueAvailable(ConfigurationContainer.Local, $"feed{0}"))
            {
                ReadFeedsFromConfiguration();
            }
            else  AddDefaultFeeds();
            return base.InitializeAsync();
        }

        protected override Task<bool> SaveAsync()
        {
            //store all feed inside configuration
            for (int i = 0; i<Feeds.Count; i++)
            {
                configurationService.SetValue(ConfigurationContainer.Local, $"feed{i}", Feeds[i]);
            }

            return base.SaveAsync();
        }

        private void ReadFeedsFromConfiguration()
        {
            NugetFeed temp = null; ;
            int i = 0;

            //restore values from configuration
            while(configurationService.IsLocalValueAvailable($"feed{i}"))
            {
                temp = configurationService.GetValue(ConfigurationContainer.Local, $"feed{i}");

                if (temp != null)
                {
                    Feeds.Add(temp);
                }
                else log.Error($"Configuration value under key {i} is broken and cannot be loaded");
                i++;
            }
        }

        #region command actions

        private void OnRemoveFeed()
        {
            Feeds.Remove(SelectedFeed);
        }

        private void OnMoveUpFeed()
        {
            Feeds.MoveUp(SelectedFeed);
        }

        private void OnMoveDownFeed()
        {
            Feeds.MoveDown(SelectedFeed);
        }

        private void OnAddFeed()
        {
            Feeds.Add(new NugetFeed(namePlaceholder, sourcePlaceholder));
        }

        #endregion 

        private void AddDefaultFeeds()
        {
            string defaultNugetOrgUrl = "https://api.nuget.org/v3/index.json";
            string defaultNugetOrgName = "nuget.org";

            Feeds.Add(new NugetFeed(defaultNugetOrgName, defaultNugetOrgUrl));

        }

        const string namePlaceholder = "Package source";
        const string sourcePlaceholder = "http://packagesource";
        private readonly NugetConfigurationService configurationService;
        private static readonly ILog log = LogManager.GetCurrentClassLogger();
    }
}
