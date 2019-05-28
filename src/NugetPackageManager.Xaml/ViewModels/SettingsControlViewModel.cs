using Catel.MVVM;
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

        public Command UpdateFeed { get; set; }

        public Command RemoveFeed { get; set; }

        public Command MoveUpFeed { get; set; }

        public Command MoveDownFeed { get; set; }

        public Command AddFeed { get; set; }

        public Command OpenChooseLocalPathToSourceDialog { get; set; }

        #endregion

        public ObservableCollection<NugetFeed> Feeds { get; set; } = new ObservableCollection<NugetFeed>();

        public string Name { get; set; }

        public string Source { get; set; }

        public NugetFeed SelectedFeed { get; set; }

        protected void CommandInitialize()
        {
            UpdateFeed = new Command(OnSaveOrUpdateFeed);
            RemoveFeed = new Command(OnRemoveFeed);
            MoveUpFeed = new Command(OnMoveUpFeed);
            MoveDownFeed = new Command(OnMoveDownFeed);
            AddFeed = new Command(OnAddFeed);
            OpenChooseLocalPathToSourceDialog = new Command(OnOpenChooseLocalPathToSourceDialog);
        }


        #region command actions

        private void OnSaveOrUpdateFeed()
        {

        }

        private void OnRemoveFeed()
        {
            Feeds.Remove(SelectedFeed);
        }

        private void OnMoveUpFeed()
        {
     
        }

        private void OnMoveDownFeed()
        {

        }

        private void OnAddFeed()
        {
            Feeds.Add(new NugetFeed(namePlaceholder, sourcePlaceholder));
        }

        private void OnOpenChooseLocalPathToSourceDialog()
        {

        }

        #endregion 

        const string namePlaceholder = "Package source";
        const string sourcePlaceholder = "http://packagesource";
    }
}
