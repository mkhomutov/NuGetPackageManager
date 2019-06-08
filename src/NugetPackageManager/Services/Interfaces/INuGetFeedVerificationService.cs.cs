using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface INuGetFeedVerificationService
    {
        FeedVerificationResult VerifyFeed(string source, bool authenticateIfRequired = true);
    }
}
