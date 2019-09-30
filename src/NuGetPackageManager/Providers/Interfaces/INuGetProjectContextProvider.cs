using NuGet.ProjectManagement;

namespace NuGetPackageManager.Providers
{
    public interface INuGetProjectContextProvider
    {
        INuGetProjectContext GetProjectContext(FileConflictAction fileConflictAction);
    }
}