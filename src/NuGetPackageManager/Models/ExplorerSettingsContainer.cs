namespace NuGetPackageManager.Models
{
    using Catel.Data;
    using System.Collections.Generic;

    public class ExplorerSettingsContainer : ModelBase
    {
        public List<NuGetFeed> NuGetFeeds { get; set; } = new List<NuGetFeed>();

        public NuGetFeed ObservedFeed { get; set; }

        public bool IsPreReleaseIncluded { get; set; }

        public string SearchString { get; set; }
    }
}
