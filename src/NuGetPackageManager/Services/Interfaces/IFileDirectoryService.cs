using System.Collections.Generic;

namespace NuGetPackageManager.Services
{
    public interface IFileDirectoryService
    {
        void DeleteDirectoryTree(string folderPath, out List<string> failedEntries);
        string GetGloabalPackagesFolder();
    }
}