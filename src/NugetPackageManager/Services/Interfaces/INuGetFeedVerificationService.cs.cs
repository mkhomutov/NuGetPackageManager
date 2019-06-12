namespace NuGetPackageManager.Services
{
    using System.Threading.Tasks;

    public interface INuGetFeedVerificationService
    {
        Task<FeedVerificationResult> VerifyFeedAsync(string source, bool authenticateIfRequired = true);

        //[ObsoleteEx]
        FeedVerificationResult VerifyFeed(string source, bool authenticateIfRequired = true);
    }
}
