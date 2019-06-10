using Catel;
using NuGet.Configuration;
using NuGet.Credentials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Providers
{
    public class NativeWinCredentialsProvider : ICredentialProvider
    {
        private readonly IAuthenticationProvider _authenticationProvider;

        public NativeWinCredentialsProvider(IAuthenticationProvider authenticationProvider)
        {
            Argument.IsNotNull(() => authenticationProvider);
            _authenticationProvider = authenticationProvider;
        }

        public string Id => throw new NotImplementedException();

        public async Task<CredentialResponse> GetAsync(Uri uri, IWebProxy proxy, CredentialRequestType type, string message, bool isRetry, bool nonInteractive, CancellationToken cancellationToken)
        {
            //todo add credentials cache

            var authCredentials = await _authenticationProvider.GetCredentialsAsync(uri, isRetry);

            //creating network credentials
            var nugetCredentials = new NetworkCredential(authCredentials.UserName, authCredentials.Password);


            var response = new CredentialResponse(nugetCredentials);

            return response;
        }
    }
}
