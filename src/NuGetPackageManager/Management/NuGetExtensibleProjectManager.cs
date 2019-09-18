namespace NuGetPackageManager.Management
{
    using NuGet.Packaging.Core;
    using NuGet.ProjectManagement;

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
