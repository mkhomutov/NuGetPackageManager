using Catel.Data;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public class NuGetPackage : ModelBase
    {
        private readonly IPackageSearchMetadata _packageMetadata;

        public NuGetPackage(IPackageSearchMetadata packageMetadata)
        {
            Title = packageMetadata.Title;
            Description = packageMetadata.Description;

            _packageMetadata = packageMetadata;

            LastVersion = packageMetadata.Identity.Version;
        }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public IEnumerable<NuGetVersion> Versions { get; private set; }

        public NuGetVersion LastVersion { get; private set; }

        public async Task<IEnumerable<NuGetVersion>> LoadVersionsAsync()
        {
            var versinfo = await _packageMetadata.GetVersionsAsync();

            Versions = versinfo.Select(x => x.Version).OrderByDescending(x => x);

            return Versions;
        }
    }
}
