using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management.EventArgs
{
    public class NuGetProjectEventArgs : System.EventArgs
    {
        public NuGetProjectEventArgs(IExtensibleProject project, PackageIdentity package)
        {
            Project = project;
            Package = package;
        }

        public IExtensibleProject Project { get; }
        public PackageIdentity Package { get; }
    }
}
