using NuGet.Protocol.Core.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management
{
    public class SourceContext
    {
        public bool IsMultipleRepository => SourceRepositories?.Count > 1;

        public IReadOnlyList<SourceRepository> SourceRepositories { get; private set; }
    }
}
