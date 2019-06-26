namespace NuGetPackageManager.Models
{
    using System.Collections.Generic;

    public class ExplorerSettingsContainer
    {
        public List<NuGetFeed> NuGetFeeds { get; set; } = new List<NuGetFeed>();

        public bool IsPreReleaseIncluded { get; set; }
    }
}
