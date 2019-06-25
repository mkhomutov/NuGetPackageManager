using Catel.MVVM;
using Catel.Threading;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.ViewModels
{
    public class ExplorerPageViewModel : ViewModelBase
    {
        int pageSize = 17;
        int lastLoaded = 0;

        public ExplorerPageViewModel(string pageTitle)
        {
            Title = pageTitle;

            LoadNextPackagePage = new TaskCommand(LoadNextPackagePageExecute);
        }

        protected async override Task InitializeAsync()
        {
            _packages = new ObservableCollection<IPackageSearchMetadata>();
            await LoadPackagesForTestAsync(0);
        }

        /// <summary>
        /// Example set of items
        /// </summary>
        public ObservableCollection<IPackageSearchMetadata> Packages
        {
            get { return _packages; }
            set
            {
                _packages = value;
                RaisePropertyChanged(() => Packages);
            }
        }

        #region commands

        public TaskCommand LoadNextPackagePage { get; set; }

        private async Task LoadNextPackagePageExecute()
        {
            await LoadPackagesForTestAsync(lastLoaded + 1);
        }

        #endregion


        private ObservableCollection<IPackageSearchMetadata> _packages { get; set; }

        private async Task LoadPackagesForTestAsync(int start)
        {
            var v3_providers = Repository.Provider.GetCoreV3();

            var packageSource = new PackageSource("https://api.nuget.org/v3/index.json");

            var repoProvider = new SourceRepositoryProvider(Settings.LoadDefaultSettings(root: null), v3_providers);

            var repository = new SourceRepository(packageSource, v3_providers);

            var searchResource = await repository.GetResourceAsync<PackageSearchResource>();

            using (var cts = new CancellationTokenSource())
            {
                var cancellationToken = cts.Token;

                //try to perform search
                var packages = await searchResource.SearchAsync(String.Empty, new SearchFilter(false), 0, pageSize, new Loggers.DebugLogger(true), cancellationToken);

                foreach(var p in packages)
                {
                    Packages.Add(p);
                }

                lastLoaded = Packages.Count - 1;
            }
        }
    }
}
