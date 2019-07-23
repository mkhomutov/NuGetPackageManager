using NuGet.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;


namespace NuGetPackageManager
{
    public class PackageSourceWrapper
    {
        public IReadOnlyList<PackageSource> PackageSources { get; private set; }

        bool IsMultipleSource => PackageSources.Count > 1;

        public PackageSourceWrapper(string source)
        {
            PackageSources = new List<PackageSource>() { new PackageSource(source) };
        }

        public PackageSourceWrapper(IReadOnlyList<string> sources)
        {
            PackageSources = new List<PackageSource>(sources.Select(x => new PackageSource(x)));
        }
    }
}
