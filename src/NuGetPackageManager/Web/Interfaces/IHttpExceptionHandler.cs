using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Web
{
    public interface IHttpExceptionHandler<T>
    {
        FeedVerificationResult HandleException(T exception, string source);
    }
}
