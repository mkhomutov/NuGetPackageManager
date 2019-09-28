using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management.EventArgs
{
    public class InstallNuGetProjectEventArgs : NuGetProjectEventArgs
    {
        public InstallNuGetProjectEventArgs(IExtensibleProject project, PackageIdentity package, bool result) : base(project, package)
        {
            Result = result;
        }

        public bool Result { get; }
    }

    public class BatchedInstallNuGetProjectEventArgs : InstallNuGetProjectEventArgs
    {
        public BatchedInstallNuGetProjectEventArgs(InstallNuGetProjectEventArgs eventArgs) : base(eventArgs.Project, eventArgs.Package, eventArgs.Result)
        {

        }

        public bool IsBatchEnd { get; set; }
    }
}
