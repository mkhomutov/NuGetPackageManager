using NuGetPackageManager.Models;
using System.Collections.Generic;

namespace NuGetPackageManager
{
    public interface IExtensibleProject
    {
        string Name { get; }

        string Framework { get; }

        string ContentPath { get; }

        IReadOnlyList<NuGetPackage> PackageList { get; }

        void Install();

        void Update();

        void Uninstall();
    }
}
