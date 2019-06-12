namespace NuGetPackageManager.Services
{
    using Catel;
    using Catel.Logging;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Credentials;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Loggers;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class NuGetFeedVerificationService : INuGetFeedVerificationService
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();
        private readonly ICredentialProviderLoaderService _credentialProviderLoaderService;

        public NuGetFeedVerificationService(ICredentialProviderLoaderService credentialProviderLoaderService)
        {
            Argument.IsNotNull(() => credentialProviderLoaderService);
            _credentialProviderLoaderService = credentialProviderLoaderService;

            //set own provider 
            HttpHandlerResourceV3.CredentialService = new Lazy<ICredentialService>(() => new ExplorerCredentialService(
                    new AsyncLazy<IEnumerable<ICredentialProvider>>(() => _credentialProviderLoaderService.GetCredentialProvidersAsync()),
                    false,
                    true)
                );
        }

        public async Task<FeedVerificationResult> VerifyFeedAsync(string source, bool authenticateIfRequired = true)
        {
            Argument.IsNotNull(() => source);

            var result = FeedVerificationResult.Valid;
            var logger = new DebugLogger(true);

            StringBuilder errorMessage = new StringBuilder($"Failed to verify feed '{source}'");

            _log.Debug("Verifying feed '{0}'", source);

            var v3_providers = Repository.Provider.GetCoreV3();
            try
            {
                var packageSource = new PackageSource(source);

                var repoProvider = new SourceRepositoryProvider(Settings.LoadDefaultSettings(root: null), Repository.Provider.GetCoreV3());

                var repository = new SourceRepository(packageSource, v3_providers);

                var searchResource = await repository.GetResourceAsync<PackageSearchResource>();

                //try to perform search
                var metadata = await searchResource.SearchAsync(String.Empty, new SearchFilter(false), 0, 1, logger, CancellationToken.None);
            }
            catch (FatalProtocolException ex)
            {
                result = HandleNugetProtocolException(ex, source);
            }
            catch (WebException ex)
            {
                result = HandleWebException(ex, source);
            }
            catch (UriFormatException ex)
            {
                errorMessage.Append(", a UriFormatException occurred");
                _log.Debug(ex, errorMessage.ToString());

                result = FeedVerificationResult.Invalid;
            }
            catch (Exception ex)
            {
                _log.Debug(ex, errorMessage.ToString());

                result = FeedVerificationResult.Invalid;
            }

            _log.Debug("Verified feed '{0}', result is '{1}'", source, result);

            return result;
        }

        private static FeedVerificationResult HandleWebException(WebException exception, string source)
        {
            try
            {
                var httpWebResponse = (HttpWebResponse)exception.Response;
                if (ReferenceEquals(httpWebResponse, null))
                {
                    return FeedVerificationResult.Invalid;
                }

                //403 error
                if (httpWebResponse.StatusCode == HttpStatusCode.Forbidden)
                {
                    return FeedVerificationResult.AuthorizationRequired;
                }

                //401 error
                if (httpWebResponse.StatusCode == HttpStatusCode.Unauthorized)
                {
                    return FeedVerificationResult.AuthenticationRequired;
                }
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Failed to verify feed '{0}'", source);
            }

            return FeedVerificationResult.Invalid;
        }

        private static FeedVerificationResult HandleNugetProtocolException(FatalProtocolException exception, string source)
        {
            try
            {
                var innerException = exception.InnerException;

                if (innerException == null)
                {
                    //handle based on protocol error messages
                    if (exception.Message.Contains("returned an unexpected status code '401 Unauthorized'"))
                    {
                        return FeedVerificationResult.AuthenticationRequired;
                    }
                }
                else
                {
                    if (innerException is WebException)
                    {
                        HandleWebException(innerException as WebException, source);
                    }
                }

            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Failed to verify feed '{0}'", source);
            }

            return FeedVerificationResult.Invalid;
        }

        [ObsoleteEx]
        public Task<FeedVerificationResult> VerifyFeed(string source, bool authenticateIfRequired = true)
        {
            throw new NotImplementedException();
        }
    }
}
