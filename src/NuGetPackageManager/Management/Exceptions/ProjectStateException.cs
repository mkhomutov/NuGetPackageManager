using System;

namespace NuGetPackageManager.Management.Exceptions
{
    public class ProjectStateException : Exception
    {
        public ProjectStateException(string message)
            : base(message)
        {
        }
    }
}
