using Catel.MVVM;
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
        public SettingsControlViewModel()
        {
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
            AddDefaultFeeds();
            return base.InitializeAsync();
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
    }
}
