using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
