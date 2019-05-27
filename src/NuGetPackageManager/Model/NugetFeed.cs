using Catel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Model
{
    public class NugetFeed : ModelBase
    {
        public NugetFeed(string name, string source)
        {
            Name = name;
            Source = source;
        }

        public string Name { get; set; }

        public string Source { get; set; }

        public bool IsActive { get; set; }
    }
}
