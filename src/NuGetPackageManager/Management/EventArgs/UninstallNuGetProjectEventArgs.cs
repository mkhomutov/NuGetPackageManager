using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGet.Packaging.Core;

namespace NuGetPackageManager.Management.EventArgs
{
    public class UninstallNuGetProjectEventArgs : NuGetProjectEventArgs
    {
        public UninstallNuGetProjectEventArgs(IExtensibleProject project, PackageIdentity package, bool result) : base(project, package)
        {
            Result = result;
        }

        public bool Result { get; }
    }
}
