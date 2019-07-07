namespace NuGetPackageManager.Models
{
    using Catel.Data;
    using System.Collections.Generic;

    public class ExplorerSettingsContainer : ModelBase
    {
        public List<NuGetFeed> NuGetFeeds { get; set; } = new List<NuGetFeed>();

        public bool IsPreReleaseIncluded { get; set; }
    }
}
