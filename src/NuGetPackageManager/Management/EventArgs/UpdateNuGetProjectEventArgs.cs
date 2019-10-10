﻿namespace NuGetPackageManager.Management.EventArgs
{
    using NuGet.Packaging.Core;
    using NuGet.Versioning;
    using System.Collections.Generic;

    public class UpdateNuGetProjectEventArgs : NuGetProjectEventArgs
    {
        public UpdateNuGetProjectEventArgs(IExtensibleProject project, PackageIdentity package) : base(project, package)
        {
            RemovedVersions = new List<NuGetVersion>();
        }

        public UpdateNuGetProjectEventArgs(IExtensibleProject project, PackageIdentity beforeUpdate, IEnumerable<NuGetProjectEventArgs> updateEventArgs) : this(project, beforeUpdate)
        {
            foreach (var arg in updateEventArgs)
            {
                if (arg is UninstallNuGetProjectEventArgs)
                {
                    RemovedVersions.Add(arg.Package.Version);
                }

                if (arg is InstallNuGetProjectEventArgs)
                {
                    InstalledVersion = arg.Package.Version;
                }
            }
        }

        public List<NuGetVersion> RemovedVersions { get; set; }

        public NuGetVersion InstalledVersion { get; set; }
    }
}
