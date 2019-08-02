using NuGet.Protocol.Core.Types;

namespace NuGetPackageManager.Web
{
    public static class FatalProtocolExceptionExtension
    {
        public static bool HidesUnauthorizedError(this FatalProtocolException fatalProtocolException)
        {
            return fatalProtocolException.Message.Contains("returned an unexpected status code '401 Unauthorized'");
        }

        public static bool HidesForbiddenError(this FatalProtocolException fatalProtocolException)
        {
            return fatalProtocolException.Message.Contains("returned an unexpected status code '403 Forbidden'");
        }
    }
}
