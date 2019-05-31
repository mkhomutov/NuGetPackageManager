using Catel;
using Catel.Data;
using Catel.MVVM;
using Catel.Services;
using Catel.Threading;
using Microsoft.WindowsAPICodePack.Dialogs;
using NugetPackageManager.Xaml.Providers;
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

        public FeedDetailViewModel(NugetFeed feed, IModelProvider<NugetFeed> modelProvider)
        {
            Argument.IsNotNull(() => feed);
            Feed = feed;

            //work with model clone

            Feed = (NugetFeed)feed.Clone();

            this.modelProvider = modelProvider;

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

        protected override Task<bool> SaveAsync()
        {
            return Task.FromResult(true);            
        }

        #region command actions

        private void OnSaveOrUpdateFeed()
        {
            //manually save model and pass forward
            Feed.ForceEndEdit();
            modelProvider.Model = Feed;
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

        private readonly IModelProvider<NugetFeed> modelProvider;
    }
}
