namespace NuGetPackageManager.Management
{
    using NuGetPackageManager.Models;
    using System;
    using System.Collections.Generic;

    public class ExamplePackageManagement : IExtensibleProject
    {
        public string Name => "Plain project extensible example";

        public IReadOnlyList<NuGetPackage> PackageList => throw new NotImplementedException();

        public string Framework => ".NETFramework,Version=v4.5";

        public void Install()
        {
        }

        public void Uninstall()
        {
        }

        public void Update()
        {
        }

        public ExamplePackageManagement()
        {
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
