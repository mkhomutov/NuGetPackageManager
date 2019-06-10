using NuGetPackageManager.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Providers
{
    public interface IAuthenticationProvider
    {
        #region Methods
        Task<AuthenticationCredentials> GetCredentialsAsync(Uri uri, bool previousCredentialsFailed);
        #endregion
    }
}
