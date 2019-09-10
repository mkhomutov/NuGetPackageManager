namespace NuGetPackageManager.Management.Exceptions
{
    using System;

    public class ProjectStateException : Exception
    {
        public ProjectStateException(string message)
            : base(message)
        {
        }
    }
}
