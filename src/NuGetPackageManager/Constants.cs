namespace NuGetPackageManager
{
    public static class Constants
    {
        public const string NamePlaceholder = "Package source";

        public const string SourcePlaceholder = "http://packagesource";

        public const string DefaultNugetOrgUri = "https://api.nuget.org/v3/index.json";

        public const string DefaultNugetOrgName = "nuget.org";

        public const string ConfigKeySeparator = "|";

        public const string PackageInstallationConflictMessage = "Conflict during package installation";

        public const string ProductName = "NuGetPackageManager";

        public const string CompanyName = "WildGums";

        public const string PackageManagement = "Package management";

        public static class Messages
        {
            public const string CacheClearEndedSuccessful = "NuGet cache cleared";
            public const string CachedClearEndedWithError = "NuGet cache cleared with some errors. You can see details in log";
            public const string CacheClearFailed = "Fatal error during cache clearing";
        }
    }
}
