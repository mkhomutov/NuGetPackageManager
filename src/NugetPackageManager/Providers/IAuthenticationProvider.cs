namespace NuGetPackageManager.Providers
{
    using NuGetPackageManager.Models;
    using System;
    using System.Threading.Tasks;

    public interface IAuthenticationProvider
    {
        Task<AuthenticationCredentials> GetCredentialsAsync(Uri uri, bool previousCredentialsFailed);
    }
}
