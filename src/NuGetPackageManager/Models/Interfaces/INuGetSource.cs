namespace NuGetPackageManager.Models
{
    public interface INuGetSource
    {
        string Name { get; }

        string Source { get; }

        PackageSourceWrapper GetPackageSource();

        bool IsAccessible { get; }

        bool IsVerified { get; }
    }
}
