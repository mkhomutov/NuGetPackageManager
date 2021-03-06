﻿namespace NuGetPackageManager
{
    using NuGet.Configuration;
    using NuGet.Protocol;
    using System;

    public static class HttpHandlerResourceV3Extensions
    {
        public static T GetCredentialServiceImplementation<T>(this HttpHandlerResourceV3 httpResourceHandler) where T : class, ICredentialService
        {
            if (HttpHandlerResourceV3.CredentialService != null)
            {
                return HttpHandlerResourceV3.CredentialService.Value as T;
            }

            throw new InvalidOperationException();
        }
    }
}
