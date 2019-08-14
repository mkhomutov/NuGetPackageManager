using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager
{
    public class TaskResultOrException<T>
    {
        public TaskResultOrException(T result)
        {
            IsSuccess = true;
            Result = result;
        }

        public TaskResultOrException(Exception ex)
        {
            IsSuccess = false;
            Exception = ex;
        }

        public bool IsSuccess { get; }
        public T Result { get; }
        public Exception Exception { get; }

        public T UnwrapResult()
        {
            return Result;
        }
    }
}
