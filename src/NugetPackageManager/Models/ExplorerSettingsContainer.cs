using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public class ExplorerSettingsContainer
    {
        public List<NuGetFeed> NuGetFeeds { get; set; } = new List<NuGetFeed>();

        public bool IsPreReleaseIncluded { get; set; }
    }
}
