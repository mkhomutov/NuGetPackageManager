namespace NuGetPackageManager.Management.EventArgs
{
    using NuGet.Packaging.Core;

    public class UpdateNuGetProjectEventArgs : NuGetProjectEventArgs
    {
        public UpdateNuGetProjectEventArgs(IExtensibleProject project, PackageIdentity package) : base(project, package)
        {
        }
    }
}
