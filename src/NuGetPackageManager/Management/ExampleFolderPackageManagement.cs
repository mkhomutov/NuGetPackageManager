using NuGetPackageManager.Models;
using System;
using System.Collections.Generic;

namespace NuGetPackageManager.Management
{
    public class ExampleFolderPackageManagement : IManagerExtensible
    {
        public string Name => "Test_Folder";

        public IReadOnlyList<NuGetPackage> PackageList { get; set; }

        public void Install()
        {
            throw new NotImplementedException();
        }

        public void Uninstall()
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            throw new NotImplementedException();
        }
    }
}
