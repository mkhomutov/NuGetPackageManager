namespace NuGetPackageManager.ViewModels
{
    using Catel;
    using Catel.Collections;
    using Catel.Configuration;
    using Catel.Data;
    using Catel.Logging;
    using Catel.MVVM;
    using Catel.Services;
    using NuGetPackageManager.Models;
    using NuGetPackageManager.Providers;
    using NuGetPackageManager.Services;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class SettingsControlViewModel : ViewModelBase
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        private readonly NugetConfigurationService _configurationService;

        private readonly INuGetFeedVerificationService _feedVerificationService;

        private readonly IModelProvider<NuGetFeed> _modelProvider;

        public SettingsControlViewModel(List<NuGetFeed> configredFeeds, IConfigurationService configurationService, INuGetFeedVerificationService feedVerificationService,
            IModelProvider<NuGetFeed> modelProvider)
        {
            Argument.IsNotNull(() => configurationService);
            Argument.IsNotNull(() => modelProvider);
            Argument.IsNotNull(() => feedVerificationService);
            Argument.IsNotNull(() => configredFeeds);

            ActiveFeeds = configredFeeds;

            _configurationService = configurationService as NugetConfigurationService;
            _feedVerificationService = feedVerificationService;
            _modelProvider = modelProvider;
            CommandInitialize();
            Title = "Settings";

            DeferValidationUntilFirstSaveCall = true;
        }

        public ObservableCollection<NuGetFeed> Feeds { get; set; } = new ObservableCollection<NuGetFeed>();

        [Model]
        public NuGetFeed SelectedFeed { get; set; }

        /// <summary>
        /// Feeds which should be visible for NuGet Package Manager
        /// </summary>
        public List<NuGetFeed> ActiveFeeds { get; set; }

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

        protected void CommandInitialize()
        {
            RemoveFeed = new Command(OnRemoveFeedExecute);
            MoveUpFeed = new Command(OnMoveUpFeedExecute);
            MoveDownFeed = new Command(OnMoveDownFeedExecute);
            AddFeed = new Command(OnAddFeedExecute);
        }

        protected override Task InitializeAsync()
        {
            //handle manual model save on child viewmodel
            _modelProvider.PropertyChanged += OnModelProviderPropertyChanged;
            Feeds.CollectionChanged += OnFeedsCollectioChanged;

            return base.InitializeAsync();
        }

        protected override Task<bool> SaveAsync()
        {
            //store all feed inside configuration
            for (int i = 0; i < Feeds.Count; i++)
            {
                _configurationService.SetValue(ConfigurationContainer.Local, $"feed{i}", Feeds[i]);
            }

            //send usable feeds (including failed)
            ActiveFeeds.Clear();
            ActiveFeeds.AddRange(Feeds);

            return base.SaveAsync();
        }

        protected override Task CloseAsync()
        {
            _modelProvider.PropertyChanged -= OnModelProviderPropertyChanged;
            return base.CloseAsync();
        }

        protected override void ValidateBusinessRules(List<IBusinessRuleValidationResult> validationResults)
        {
            if (SelectedFeed != null)
            {
                if (IsNamesNotUniqueRule(out var names))
                {
                    foreach(var name in names)
                    {
                        validationResults.Add(BusinessRuleValidationResult.CreateError($"Two or more feeds have same name '{name}'"));
                    }
                }
            }
        }

        private bool IsNamesNotUniqueRule(out IEnumerable<string> invalidNames)
        {
            var names = new List<string>();

            var groups = Feeds.GroupBy(x => x.Name).Where(g => g.Count() > 1);

            groups.ForEach(g => names.Add(g.Key));

            invalidNames = names;

            return groups.Count() > 0;
        }

        private async Task VerifyFeedAsync(NuGetFeed feed)
        {
            if (feed == null || !feed.IsValid())
            {
                return;
            }

            feed.IsVerifiedNow = true;

            var result = await _feedVerificationService.VerifyFeedAsync(feed.Source, true);

            feed.VerificationResult = result;

            feed.IsVerifiedNow = false;
        }


        private void OnModelProviderPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //should drop current selected row and add updated
            Feeds.Remove(SelectedFeed);
            Feeds.Add(_modelProvider.Model);

            SelectedFeed = _modelProvider.Model;

            //keep selection
            //SelectedFeed = _modelProvider.Model;

            ////copy values from edited clone
            //_modelProvider.Model.CopyTo(SelectedFeed);

            //_modelProvider.Model = SelectedFeed;
        }


        private async void OnFeedsCollectioChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //verify all new feeds in collection
            //because of feed edit is simple re-insertion we should'nt handle property change inside model
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (NuGetFeed item in e.NewItems)
                    {
                        if (!item.IsLocal())
                        {
                            await VerifyFeedAsync(item);
                        }
                    }
                }
            }
        }
    }
}