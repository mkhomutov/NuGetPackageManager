using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface INuGetFeedVerificationService
    {
        Task<FeedVerificationResult> VerifyFeedAsync(string source, bool authenticateIfRequired = true);
    }
}
