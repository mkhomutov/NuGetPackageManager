using Catel;
using Catel.Logging;
using NuGet.Common;
using NuGet.Configuration;
using NuGetPackageManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Cache
{
    public class NuGetCacheManager : INuGetCacheManager
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private readonly IFileDirectoryService _fileDirectoryService;

        public NuGetCacheManager(IFileDirectoryService fileDirectoryService)
        {
            Argument.IsNotNull(() => fileDirectoryService);
            _fileDirectoryService = fileDirectoryService;
        }

        public void ClearAll()
        {
            ClearNuGetFoler(SettingsUtility.GetHttpCacheFolder(), "Http-cache");
            ClearNuGetFoler(_fileDirectoryService.GetGloabalPackagesFolder(), "Global-packages");
            ClearNuGetFoler(NuGetEnvironment.GetFolderPath(NuGetFolderPath.Temp), "Temp");

            Log.Info("Cache clearing operation finished");
        }

        private bool ClearNuGetFoler(string folderPath, string folderDescription)
        {
            var success = true;

            if (!string.IsNullOrEmpty(folderPath))
            {
                Log.Info($"Clear {folderDescription} folder on path {folderPath}");

                success &= ClearCacheDirectory(folderPath);
            }

            return success;
        }

        private bool ClearCacheDirectory(string folderPath)
        {
            List<string> failedDeletes = new List<string>();

            try
            {
                _fileDirectoryService.DeleteDirectoryTree(folderPath, out failedDeletes);
            }
            catch (IOException)
            {
                Log.Error("Cache clear ended unsuccessfully, directory in use by another process");
            }
            finally
            {
                //log all errors

                if (failedDeletes.Any())
                {
                    Log.Info("Some directories cannot be deleted, directory tree was partially cleared:");

                    foreach (var failedDelete in failedDeletes.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                    {
                        Log.Info($"Failed to delete path {failedDelete}");
                    }
                }
            }

            return !failedDeletes.Any();
        }
    }
}
