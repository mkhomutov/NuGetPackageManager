﻿namespace NuGetPackageManager.Services
{
    using Catel;
    using Catel.Logging;
    using NuGet.Common;
    using NuGet.Configuration;
    using NuGet.Credentials;
    using NuGet.Protocol;
    using NuGet.Protocol.Core.Types;
    using NuGetPackageManager.Loggers;
    using NuGetPackageManager.Web;
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    internal class NuGetFeedVerificationService : INuGetFeedVerificationService
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        private static readonly IHttpExceptionHandler<WebException> webExceptionHandler = new HttpWebExceptionHandler();
        private static readonly IHttpExceptionHandler<FatalProtocolException> fatalProtocolExceptionHandler = new FatalProtocolExceptionHandler();

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

        public async Task<FeedVerificationResult> VerifyFeedAsync(string source, CancellationToken ct, bool authenticateIfRequired = true)
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

                var repoProvider = new SourceRepositoryProvider(Settings.LoadDefaultSettings(root: null), v3_providers);

                var repository = repoProvider.CreateRepository(packageSource);

                var searchResource = await repository.GetResourceAsync<PackageSearchResource>();

                var httpHandler = await repository.GetResourceAsync<HttpHandlerResourceV3>();


                //maybe use Task<Tuple<bool, INuGetResource>> TryCreate(SourceRepository source, CancellationToken token) instead

                //try to perform search
                try
                {
                    var metadata = await searchResource.SearchAsync(String.Empty, new SearchFilter(false), 0, 1, logger, ct);
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    var credentialsService = httpHandler.GetCredentialServiceImplementation<ExplorerCredentialService>();

                    if (credentialsService != null)
                    {
                        credentialsService.ClearRetryCache();
                    }
                }
            }
            catch (FatalProtocolException ex)
            {
                if (ct.IsCancellationRequested)
                {
                    //cancel operation
                    throw new OperationCanceledException("Verification was canceled", ex, ct);
                }
                result = fatalProtocolExceptionHandler.HandleException(ex, source);
            }
            catch (WebException ex)
            {
                result = webExceptionHandler.HandleException(ex, source);
            }
            catch (UriFormatException ex)
            {
                errorMessage.Append(", a UriFormatException occurred");
                _log.Debug(ex, errorMessage.ToString());

                result = FeedVerificationResult.Invalid;
            }
            catch (Exception ex) when (!ct.IsCancellationRequested)
            {
                _log.Debug(ex, errorMessage.ToString());

                result = FeedVerificationResult.Invalid;
            }

            _log.Debug("Verified feed '{0}', result is '{1}'", source, result);

            return result;
        }

        [ObsoleteEx]
        public FeedVerificationResult VerifyFeed(string source, bool authenticateIfRequired = true)
        {
            int timeOut = 3000;

            Argument.IsNotNull(() => source);

            var result = FeedVerificationResult.Valid;
            var logger = new DebugLogger(true);

            StringBuilder errorMessage = new StringBuilder($"Failed to verify feed '{source}'");

            _log.Debug("Verifying feed '{0}'", source);

            var v3_providers = Repository.Provider.GetCoreV3();
            try
            {
                var packageSource = new PackageSource(source);

                //var repoProvider = new SourceRepositoryProvider(Settings.LoadDefaultSettings(root: null), Repository.Provider.GetCoreV3());

                var repository = new SourceRepository(packageSource, v3_providers);

                var searchResource = repository.GetResource<PackageSearchResource>();

                using (var cts = new CancellationTokenSource())
                {
                    var cancellationToken = cts.Token;

                    //try to perform search
                    var searchTask = searchResource.SearchAsync(String.Empty, new SearchFilter(false), 0, 1, logger, cancellationToken);

                    var searchCompletion = Task.WhenAny(searchTask, Task.Delay(timeOut, cancellationToken)).Result;

                    if (searchCompletion != searchTask)
                    {
                        throw new TimeoutException("Search operation has timed out");
                    }

                    if (searchTask.IsFaulted && searchTask.Exception != null)
                    {
                        throw searchTask.Exception;
                    }
                    if (searchTask.IsCanceled)
                    {
                        return FeedVerificationResult.Unknown;
                    }
                }
            }
            catch (FatalProtocolException ex)
            {
                result = fatalProtocolExceptionHandler.HandleException(ex, source);
            }
            catch (WebException ex)
            {
                result = webExceptionHandler.HandleException(ex, source);
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
    }
}
