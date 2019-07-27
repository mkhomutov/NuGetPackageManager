using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Web
{
    public class HttpWebExceptionHandler : IHttpExceptionHandler<WebException>
    {
        public FeedVerificationResult HandleException(WebException exception, string source)
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
    }
}
