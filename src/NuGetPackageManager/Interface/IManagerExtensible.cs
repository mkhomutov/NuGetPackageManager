using NuGetPackageManager.Models;
using System.Collections.Generic;

namespace NuGetPackageManager
{
    public interface IManagerExtensible
    {
        string Name { get; }

        IReadOnlyList<NuGetPackage> PackageList { get; }

        void Install();

        void Update();

        void Uninstall();
    }
}
