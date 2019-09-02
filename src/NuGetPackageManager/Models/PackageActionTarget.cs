using Catel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public class PackageActionTarget : ModelBase
    {
        public IReadOnlyList<IExtensibleProject> DestinationsList { get; set; }
    }
}
