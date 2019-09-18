using Catel.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Loggers
{
    public class LogHelper
    {
        public static void LogUnclearedPaths(List<string> unclearedPaths, ILog log)
        {
            if (unclearedPaths?.Any() ?? false)
            {
                log.Info("Some directories cannot be deleted, directory tree was partially cleared:");

                foreach (var failedDelete in unclearedPaths.OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                {
                    log.Info($"Failed to delete path {failedDelete}");
                }
            }
        }
    }
}
