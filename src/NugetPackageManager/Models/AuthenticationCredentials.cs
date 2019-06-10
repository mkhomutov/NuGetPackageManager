namespace NuGetPackageManager.Models
{
    using Catel;
    using Catel.Data;
    using System;

    public class AuthenticationCredentials : ModelBase
    {
        public AuthenticationCredentials(Uri uri)
        {
            Argument.IsNotNull(() => uri);

            Host = uri.Host;
            Password = string.Empty;
        }

        public string Host { get; private set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public bool StoreCredentials { get; set; }
    }
}
