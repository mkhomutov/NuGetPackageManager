using Catel.Data;
using Catel.Logging;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using NuGetPackageManager.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public class NuGetPackage : ModelBase
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        private readonly IPackageSearchMetadata _packageMetadata;

        public NuGetPackage(IPackageSearchMetadata packageMetadata)
        {
            Title = packageMetadata.Title;
            Description = packageMetadata.Description;
            IconUrl = packageMetadata.IconUrl;
            Authors = packageMetadata.Authors;
            DownloadCount = packageMetadata.DownloadCount;
            Summary = packageMetadata.Summary;

            _packageMetadata = packageMetadata;

            LastVersion = packageMetadata.Identity.Version;
        }

        public string Title { get; private set; }

        public string Description { get; private set; }

        public string Authors { get; private set; }

        public long? DownloadCount { get; private set; }

        public string Summary { get; private set; }

        public Uri IconUrl { get; private set; }

        public PackageStatus Status { get; private set; } = PackageStatus.NotInstalled;

        public PackageIdentity Identity => _packageMetadata.Identity;

        public IEnumerable<NuGetVersion> Versions { get; private set; }

        public IEnumerable<VersionInfo> VersionsInfo { get; private set; }

        public bool IsLoaded { get; private set; }

        public NuGetVersion LastVersion { get; private set; }

        public NuGetVersion InstalledVersion { get; private set; }

        public async Task<IEnumerable<NuGetVersion>> LoadVersionsAsync()
        {
            var versinfo = await _packageMetadata.GetVersionsAsync();

            Versions = versinfo.Select(x => x.Version).OrderByDescending(x => x);
            VersionsInfo = versinfo;

            IsLoaded = true;

            return Versions;
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if(String.Equals(e.PropertyName, nameof(Status)))
            {
                Log.Info($"{Identity} status was changed from {e.OldValue} to {e.NewValue}");
            }
        }
    }
}
