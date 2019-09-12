using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Extensions
{
    public static class DownloadResourceResultExntesions
    {
        public static string GetResourceRoot(this DownloadResourceResult downloadResourceResult)
        {
            var fileStream = downloadResourceResult.PackageStream as FileStream;

            if(fileStream != null)
            {
                return fileStream.Name;
            }
            else
            {
                return downloadResourceResult.PackageSource ?? String.Empty;
            }
        }
    }
}
