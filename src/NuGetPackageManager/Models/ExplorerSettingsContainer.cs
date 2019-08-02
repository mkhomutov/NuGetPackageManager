namespace NuGetPackageManager.Models
{
    using Catel.Data;
    using System.Collections.Generic;
    using System.Linq;

    public class ExplorerSettingsContainer : ModelBase
    {
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
