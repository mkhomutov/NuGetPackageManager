using Catel;
using Catel.Logging;
using Catel.Scoping;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Loggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    internal class NuGetFeedVerificationService : INuGetFeedVerificationService
    {
        private static readonly ILog _log = LogManager.GetCurrentClassLogger();

        public FeedVerificationResult VerifyFeed(string source, bool authenticateIfRequired = true)
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

                var repository = new SourceRepository(packageSource, v3_providers);
                var searchResource =  repository.GetResource<PackageSearchResource>();
                
                //try to perform search
                searchResource.SearchAsync(String.Empty, new SearchFilter(false), 0, 1, logger, CancellationToken.None);
            }
            catch(WebException ex)
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

        #region Methods

        private static FeedVerificationResult HandleWebException(WebException exception, string source)
        {
            try
            {
                var httpWebResponse = (HttpWebResponse)exception.Response;
                if (ReferenceEquals(httpWebResponse, null))
                {
                    return FeedVerificationResult.Invalid;
                }

                if ((int)httpWebResponse.StatusCode == 403)
                {
                    return FeedVerificationResult.AuthenticationRequired;
                }

                if ((int)httpWebResponse.StatusCode == 401)
                {
                    return FeedVerificationResult.AuthorizationRequired;
                }

                if (exception.Status == WebExceptionStatus.ProtocolError)
                {
                    return httpWebResponse.StatusCode == HttpStatusCode.Unauthorized
                        ? FeedVerificationResult.AuthenticationRequired
                        : FeedVerificationResult.Invalid;
                }
            }
            catch (Exception ex)
            {
                _log.Debug(ex, "Failed to verify feed '{0}'", source);
            }

            return FeedVerificationResult.Invalid;
        }
        #endregion
    }
}
