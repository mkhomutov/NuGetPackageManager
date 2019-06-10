namespace NuGetPackageManager
{
    public enum FeedVerificationResult
    {
        Unknown,
        Valid,
        AuthenticationRequired,
        AuthorizationRequired,
        Invalid
    }
}
