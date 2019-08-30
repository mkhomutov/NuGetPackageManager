namespace NuGetPackageManager.Management
{
    using Catel.Logging;
    using NuGetPackageManager.Models;
    using System.Collections.Generic;

    public class ExampleFolderPackageManagement : IExtensibleProject
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public string Name => "Plain project extensible example with additinal logging";

        public IReadOnlyList<NuGetPackage> PackageList { get; set; }

        public void Install()
        {
            Log.Info("Installation started");
        }

        public void Uninstall()
        {
            Log.Info("Uninstall started");
        }

        public void Update()
        {
            Log.Info("Update started");
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
