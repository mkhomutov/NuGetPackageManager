namespace NuGetPackageManager.Management
{
    public interface INuGetExtensibleProjectManager
    {
        bool IsPackageInstalled(IExtensibleProject project, NuGet.Packaging.Core.PackageIdentity package);
    }
}
