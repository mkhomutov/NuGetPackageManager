using Catel;
using Catel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public class AuthenticationCredentials : ModelBase
    {
        #region Constructors
        public AuthenticationCredentials(Uri uri)
        {
            Argument.IsNotNull(() => uri);

            Host = uri.Host;
            Password = string.Empty;
        }
        #endregion

        #region Properties
        public string Host { get; private set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool StoreCredentials { get; set; }
        #endregion
    }
}
