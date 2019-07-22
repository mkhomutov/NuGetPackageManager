using Catel.Data;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public class NuGetPackage : ModelBase
    {
        public NuGetPackage(IPackageSearchMetadata packageMetadata)
        {
            Title = packageMetadata.Title;
            Description = packageMetadata.Description;
            
        }

        public string Title { get; private set; }

        public string Description { get; private set; }
    }
}
