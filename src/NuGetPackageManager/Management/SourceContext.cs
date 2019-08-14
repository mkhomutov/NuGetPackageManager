namespace NuGetPackageManager.Management
{
    using NuGet.Protocol.Core.Types;
    using System.Collections.Generic;

    public class SourceContext
    {
        public bool IsMultipleRepository => SourceRepositories?.Count > 1;

        public IReadOnlyList<SourceRepository> SourceRepositories { get; private set; }
    }
}
