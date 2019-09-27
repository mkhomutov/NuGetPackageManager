using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace NuGetPackageManager.Management.EventArgs
{
    public class UpdateNuGetProjectEventArgs : NuGetProjectEventArgs
    {
        public UpdateNuGetProjectEventArgs(IExtensibleProject project, PackageIdentity package) : base(project, package)
        {
        }
    }
}
