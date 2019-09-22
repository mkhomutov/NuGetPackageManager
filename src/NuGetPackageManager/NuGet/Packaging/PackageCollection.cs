using NuGet.ProjectManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuGetPackageManager.Packaging
{
    public sealed class PackageCollection : IEnumerable<PackageCollectionItem>
    {
        private readonly PackageCollectionItem[] _packages;
        private readonly ISet<string> _uniqueIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public PackageCollection(PackageCollectionItem[] packages)
        {
            _packages = packages;
            _uniqueIds.UnionWith(_packages.Select(p => p.Id));
        }

        public IEnumerator<PackageCollectionItem> GetEnumerator()
        {
            return ((IEnumerable<PackageCollectionItem>)_packages).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<PackageCollectionItem>)_packages).GetEnumerator();
        }

        public bool ContainsId(string packageId) => _uniqueIds.Contains(packageId);
    }
}
