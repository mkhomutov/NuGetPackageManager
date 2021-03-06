﻿namespace NuGetPackageManager.Services
{
    using Catel;
    using Catel.Configuration;
    using NuGet.Credentials;
    using NuGetPackageManager.Providers;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class CredentialProviderLoaderService : ICredentialProviderLoaderService
    {
        private IConfigurationService _configurationService;

        public CredentialProviderLoaderService(IConfigurationService configurationService)
        {
            Argument.IsNotNull(() => configurationService);

            _configurationService = configurationService;
        }

        public async Task<IEnumerable<ICredentialProvider>> GetCredentialProvidersAsync()
        {
            var providers = new List<ICredentialProvider>();

            var windowsUserProvider = new WindowsCredentialProvider(_configurationService);

            providers.Add(windowsUserProvider);

            return providers;
        }
    }
}
