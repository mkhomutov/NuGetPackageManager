namespace NuGetPackageManager.Providers
{
    using Catel;
    using Catel.Configuration;
    using Catel.Logging;
    using NuGet.Configuration;
    using NuGet.Credentials;
    using NuGetPackageManager.Native;
    using System;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    public class WindowsCredentialProvider : ICredentialProvider
    {
        private ILog _log = LogManager.GetCurrentClassLogger();

        private IConfigurationService _configurationService;

        public WindowsCredentialProvider(IConfigurationService configurationService)
        {
            Argument.IsNotNull(() => configurationService);

            _configurationService = configurationService;
        }

        public string Id => "Windows Credentials";

        public async Task<CredentialResponse> GetAsync(Uri uri, IWebProxy proxy, CredentialRequestType type, string message, bool isRetry, bool nonInteractive, CancellationToken cancellationToken)
        {
            if (isRetry)
            {
                _log.Debug($"Retrying to request credentials for '{uri}'");
            }
            else
            {
                _log.Debug($"Requesting credentials for '{uri}'");
            }


            bool? result = null;

            var uriString = uri.ToString().ToLower();

            var credentialsPrompter = new CredentialsPrompter(_configurationService)
            {
                Target = uriString,
                AllowStoredCredentials = !isRetry,
                ShowSaveCheckBox = true,
                WindowTitle = "Credentials required",
                MainInstruction = "Credentials are required to access this feed",
                Content = message,
                IsAuthenticationRequired = true
            };

            result = credentialsPrompter.ShowDialog();

            if (result ?? false)
            {
                //creating success response

                _log.Debug("Successfully requested credentials for '{0}' using user '{1}'", uri, credentialsPrompter.UserName);

                //creating network credentials
                var nugetCredentials = new NetworkCredential(credentialsPrompter.UserName, credentialsPrompter.Password);

                var response = new CredentialResponse(nugetCredentials);

                return response;
            }
            else
            {
                _log.Debug("Failed to request credentials for '{0}'", uri);
                return new CredentialResponse(CredentialStatus.UserCanceled);
            }
        }
    }
}
