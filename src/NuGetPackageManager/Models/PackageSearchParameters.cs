using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public class PackageSearchParameters
    {
        public PackageSearchParameters(bool prereleasIncluded, string searchString)
        {
            SearchString = searchString;
            IsPrereleaseIncluded = prereleasIncluded;
        }

        public bool IsPrereleaseIncluded { get; set; }

        public string SearchString { get; set; }
    }
}
