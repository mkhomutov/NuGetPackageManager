using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager
{
    public enum  FeedVerificationResult
    {
        Unknown,
        Valid,
        AuthenticationRequired,
        AuthorizationRequired,
        Invalid
    }
}
