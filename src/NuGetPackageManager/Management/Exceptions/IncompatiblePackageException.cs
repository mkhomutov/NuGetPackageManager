using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management.Exceptions
{
    public class IncompatiblePackageException : ProjectInstallException
    {
        public IncompatiblePackageException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public IncompatiblePackageException(string message) : base(message)
        {

        }
    }
}
