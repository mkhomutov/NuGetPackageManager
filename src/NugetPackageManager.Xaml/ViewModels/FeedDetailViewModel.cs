using Catel;
using Catel.MVVM;
using NuGetPackageManager.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetPackageManager.Xaml.ViewModels
{
    public class FeedDetailViewModel : ViewModelBase
    {
        #region commands

        public Command UpdateFeed { get; set; }

        public Command OpenChooseLocalPathToSourceDialog { get; set; }

        #endregion

        public FeedDetailViewModel(NugetFeed feed)
        {
            Argument.IsNotNull(() => feed);
            Feed = feed;

            UpdateFeed = new Command(OnSaveOrUpdateFeed);
            OpenChooseLocalPathToSourceDialog = new Command(OnOpenChooseLocalPathToSourceDialog);
        }

        [Model]
        public NugetFeed Feed { get; set; }

        [ViewModelToModel]
        public string Name { get; set; }

        [ViewModelToModel]
        public string Source { get; set; }

        #region command actions

        private void OnSaveOrUpdateFeed()
        {
        }

        private void OnOpenChooseLocalPathToSourceDialog()
        {

        }

        #endregion
    }
}
