using NuGet.Protocol.Core.Types;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface IPackageMetadataMediaDownloadService
    {
        Task DownloadFromAsync(IPackageSearchMetadata packageMetadata);
    }
}
