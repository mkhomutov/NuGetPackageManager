namespace NuGetPackageManager.Services
{
    using NuGet.Credentials;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface ICredentialProviderLoaderService
    {
        Task<IEnumerable<ICredentialProvider>> GetCredentialProvidersAsync();
    }
}
