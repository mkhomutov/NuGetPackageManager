namespace NuGetPackageManager.Providers
{
    using Catel;
    using Catel.Configuration;
    using Catel.Logging;
    using NuGetPackageManager.Models;
    using NuGetPackageManager.Native;
    using System;
    using System.Threading.Tasks;

    public class AuthenticationProvider : IAuthenticationProvider
    {
        private ILog _log = LogManager.GetCurrentClassLogger();

        private IConfigurationService _configurationService;

        public AuthenticationProvider(IConfigurationService configurationService)
        {
            Argument.IsNotNull(() => configurationService);

            _configurationService = configurationService;
        }

        public async Task<AuthenticationCredentials> GetCredentialsAsync(Uri uri, bool previousCredentialsFailed)
        {
            _log.Debug("Requesting credentials for '{0}'", uri);

            bool? result = null;

            var credentials = new AuthenticationCredentials(uri);
            var uriString = uri.ToString().ToLower();


            var credentialsPrompter = new CredentialsPrompter(_configurationService)
            {
                Target = uriString,
                AllowStoredCredentials = !previousCredentialsFailed,
                ShowSaveCheckBox = true,
                WindowTitle = "Credentials required",
                MainInstruction = "Credentials are required to access this feed",
                Content = string.Format("In order to continue, please enter the credentials for {0} below.", uri),
                IsAuthenticationRequired = true
            };

            result = credentialsPrompter.ShowDialog();

            if (result ?? false)
            {
                credentials.UserName = credentialsPrompter.UserName;
                credentials.Password = credentialsPrompter.Password;
            }
            else
            {
                credentials.StoreCredentials = false;
            }

            if (result ?? false)
            {
                _log.Debug("Successfully requested credentials for '{0}' using user '{1}'", uri, credentials.UserName);

                return credentials;
            }

            _log.Debug("Failed to request credentials for '{0}'", uri);

            return null;
        }
    }
}
