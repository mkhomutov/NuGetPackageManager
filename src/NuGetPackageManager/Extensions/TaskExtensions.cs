using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Extensions
{
    public static class TaskExtensions
    {
        public static async Task<TaskResultOrException<T>[]> WhenAllOrException<T>(this IEnumerable<Task<T>> tasks)
        {
            return await Task.WhenAll(tasks.Select(task => WrapResultOrException(task)));
        }

        public static async Task<TaskResultOrException<T>> WrapResultOrException<T>(this Task<T> task)
        {
            try
            {
                var result = await task;
                return new TaskResultOrException<T>(result);
            }
            catch (Exception ex)
            {
                return new TaskResultOrException<T>(ex);
            }
        }
    }
}
