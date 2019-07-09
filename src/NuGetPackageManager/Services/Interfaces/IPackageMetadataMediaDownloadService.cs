using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface IPackageMetadataMediaDownloadService
    {
        Task DownloadFromAsync(IPackageSearchMetadata packageMetadata);
    }
}
