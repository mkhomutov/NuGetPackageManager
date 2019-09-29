using NuGet.Packaging.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management.Exceptions
{
    public class ProjectInstallException : ProjectManageException
    {
        public ProjectInstallException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IEnumerable<PackageIdentity> CurrentBatch { get; set; }
    }
}
