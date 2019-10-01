namespace NuGetPackageManager.Management
{
    using NuGet.Packaging.Core;
    using System;
    using System.Collections.Generic;

    public class ProjectInstallException : ProjectManageException
    {
        public ProjectInstallException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IEnumerable<PackageIdentity> CurrentBatch { get; set; }
    }
}
