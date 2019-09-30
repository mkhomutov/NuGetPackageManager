using System;

namespace NuGetPackageManager.Management.Exceptions
{
    public class ProjectManageException : Exception
    {
        public ProjectManageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
