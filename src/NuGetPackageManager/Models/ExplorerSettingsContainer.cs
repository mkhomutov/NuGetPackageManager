namespace NuGetPackageManager.Models
{
    using Catel.Data;
    using System.Collections.Generic;

    public class ExplorerSettingsContainer : ModelBase
    {
        /// <summary>
        /// All feeds configured in application
        /// </summary>
        public List<NuGetFeed> NuGetFeeds { get; set; } = new List<NuGetFeed>();

        /// <summary>
        /// Feed currently used by explorer
        /// </summary>
        public NuGetFeed ObservedFeed { get; set; }

        public bool IsPreReleaseIncluded { get; set; }

        public string SearchString { get; set; }
    }
}
