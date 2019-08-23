namespace NuGetPackageManager.Models
{
    using Catel.Data;
    using NuGet.Configuration;
    using System.Collections.Generic;
    using System.Linq;

    public class ExplorerSettingsContainer : ModelBase, INuGetSettings
    {
        private static ExplorerSettingsContainer _instance;

        public static ExplorerSettingsContainer Singleton
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ExplorerSettingsContainer();
                }

                return _instance;
            }
        }

        private ExplorerSettingsContainer()
        {

        }

        /// <summary>
        /// All feeds configured in application
        /// </summary>
        public List<NuGetFeed> NuGetFeeds { get; set; } = new List<NuGetFeed>();

        /// <summary>
        /// Feed currently used by explorer
        /// </summary>
        public INuGetSource ObservedFeed { get; set; }

        public bool IsPreReleaseIncluded { get; set; }

        public string SearchString { get; set; }

        /// <summary>
        /// Create and retrive all unique package sources
        /// </summary>
        /// <returns></returns>
        public IReadOnlyList<PackageSource> GetAllPackageSources()
        {
            var feeds = NuGetFeeds.Select(x => new PackageSource(x.Source));
            return feeds.ToList();
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NuGetFeeds))
            {
                ObservedFeed = NuGetFeeds.FirstOrDefault();
            }
            base.OnPropertyChanged(e);
        }
    }
}
