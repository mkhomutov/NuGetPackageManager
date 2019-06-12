namespace NuGetPackageManager.Services
{
    using System.Threading.Tasks;

    public interface INuGetFeedVerificationService
    {
        Task<FeedVerificationResult> VerifyFeedAsync(string source, bool authenticateIfRequired = true);

        [ObsoleteEx]
        Task<FeedVerificationResult> VerifyFeed(string source, bool authenticateIfRequired = true);
    }
}
