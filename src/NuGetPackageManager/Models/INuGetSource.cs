using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public interface INuGetSource
    {
        string Name { get; }

        string Source { get; set; }

        bool MultipleSource { get; }
    }
}
