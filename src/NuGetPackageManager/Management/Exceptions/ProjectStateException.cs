using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
