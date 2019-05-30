using Catel;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using Microsoft.WindowsAPICodePack.Dialogs;
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
        public NugetFeed Feed
        {
            get { return GetValue<NugetFeed>(FeedProperty); }
            set {
                SetValue(FeedProperty, value);
            }
        }

        public static readonly PropertyData FeedProperty = RegisterProperty("Feed", typeof(NugetFeed));

        [ViewModelToModel]
        public string Name { get; set; }

        [ViewModelToModel]
        public string Source { get; set; }

        #region command actions

        private void OnSaveOrUpdateFeed()
        {

        }

        void OnFeedChanged()
        {

        }

        private void OnOpenChooseLocalPathToSourceDialog()
        {
            CommonOpenFileDialog folderDialog = new CommonOpenFileDialog();

            folderDialog.InitialDirectory = @"C:\Users";
            folderDialog.IsFolderPicker = true;
            if(folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                Source = folderDialog.FileName;
            }
        }

        #endregion
    }
}
