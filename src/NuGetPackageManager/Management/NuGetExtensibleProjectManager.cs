using NuGet.Commands;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.ProjectModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management
{
    public class NuGetExtensibleProjectManager : INuGetExtensibleProjectManager
    {
        public bool IsPackageInstalled(IExtensibleProject project, PackageIdentity package)
        {
            var underluyingFolderProject = new FolderNuGetProject(project.ContentPath);

            var result = underluyingFolderProject.PackageExists(package);

            return true;
        }
    }
}
