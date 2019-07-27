using Catel.Logging;
using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Web
{
    public class FatalProtocolExceptionHandler : IHttpExceptionHandler<FatalProtocolException>
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();
        private static readonly IHttpExceptionHandler<WebException> webExceptionHandler = new HttpWebExceptionHandler();

        public FeedVerificationResult HandleException(FatalProtocolException exception, string source)
        {
            try
            {
                var innerException = exception.InnerException;

                if (innerException == null)
                {
                    //handle based on protocol error messages
                    if (exception.HidesForbiddenError())
                    {
                        return FeedVerificationResult.AuthenticationRequired;
                    }
                    if (exception.HidesAuthorizationError())
                    {
                        return FeedVerificationResult.AuthorizationRequired;
                    }
                }
                else
                {
                    if (innerException is WebException)
                    {
                        webExceptionHandler.HandleException(innerException as WebException, source);
                    }
                }

            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Failed to verify feed '{0}'", source);
            }

            return FeedVerificationResult.Invalid;
        }

    }
}
