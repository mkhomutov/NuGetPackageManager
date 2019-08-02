using System;

namespace NuGetPackageManager
{
    public static class UriExtensions
    {
        public static Uri GetRootUri(this Uri uri)
        {
            return new Uri(uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.SafeUnescaped));
        }
    }
}
