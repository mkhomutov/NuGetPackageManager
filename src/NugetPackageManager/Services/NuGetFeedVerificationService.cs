using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    internal class NuGetFeedVerificationService : INuGetFeedVerificationService
    {
        public FeedVerificationResult VerifyFeed(string source, bool authenticateIfRequired = true)
        {
            throw new NotImplementedException();
        }
    }
}
