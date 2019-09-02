namespace NuGetPackageManager.Models
{
    using Catel.Data;
    using System.Collections.Generic;

    public class PackageActionTarget : ModelBase
    {
        public IReadOnlyList<IExtensibleProject> DestinationsList { get; set; }
    }
}
