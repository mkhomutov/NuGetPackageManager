using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuGetPackageManager.Models;

namespace NuGetPackageManager.Management
{
    public class ExamplePackageManagement : IExtensibleProject
    {
        public string Name => "Plain project extensible example";

        public IReadOnlyList<NuGetPackage> PackageList => throw new NotImplementedException();

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

