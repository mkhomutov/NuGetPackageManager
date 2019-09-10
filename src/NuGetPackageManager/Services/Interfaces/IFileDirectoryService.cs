namespace NuGetPackageManager.Services
{
    using System.Collections.Generic;

    public interface IFileDirectoryService
    {
        void DeleteDirectoryTree(string folderPath, out List<string> failedEntries);

        string GetGloabalPackagesFolder();
    }
}
