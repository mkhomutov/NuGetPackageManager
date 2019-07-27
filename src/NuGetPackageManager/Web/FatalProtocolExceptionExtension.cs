using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Web
{
    public static class FatalProtocolExceptionExtension
    {
        public static bool HidesForbiddenError(this FatalProtocolException fatalProtocolException)
        {
            return fatalProtocolException.Message.Contains("returned an unexpected status code '401 Unauthorized'");
        }

        public static bool HidesAuthorizationError(this FatalProtocolException fatalProtocolException)
        {
            return fatalProtocolException.Message.Contains("returned an unexpected status code '403 Forbidden'");
        }
    }
}
