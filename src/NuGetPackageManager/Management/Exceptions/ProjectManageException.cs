using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management.Exceptions
{
    public class ProjectManageException : Exception
    {
        public ProjectManageException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
