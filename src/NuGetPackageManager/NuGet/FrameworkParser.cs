using Catel.Logging;
using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager
{
    public static class FrameworkParser
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public static NuGetFramework TryParseFrameworkName(string frameworkString, IFrameworkNameProvider frameworkNameProvider)
        {
            try
            {
                return NuGetFramework.ParseFrameworkName(frameworkString, frameworkNameProvider);
            }
            catch (ArgumentException e)
            {
                Log.Error(e, "Incorrect target framework");
                throw;
            }
        }
    }
}
