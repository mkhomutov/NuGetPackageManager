using Catel.Services;
using Catel.Windows.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace NuGetPackageManager.Windows
{
    public class SynchronizeInvoker : ISynchronizeInvoke
    {
        private readonly Dispatcher _dispatcher;
        private readonly IDispatcherService _dispatcherService;

        public SynchronizeInvoker(Dispatcher dispatcher, IDispatcherService dispatcherService)
        {
            _dispatcher = dispatcher;
            _dispatcherService = dispatcherService;
        }

        public bool InvokeRequired => !_dispatcher.CheckAccess();

        public IAsyncResult BeginInvoke(Delegate method, object[] args)
        {
            return new DispatcherAsyncResult(
                _dispatcher.BeginInvoke(
                    method, 
                    DispatcherPriority.Normal, 
                    args));
        }

        public object EndInvoke(IAsyncResult result)
        {
            DispatcherAsyncResult dispatcherResult = result as DispatcherAsyncResult;
            dispatcherResult.Operation.Wait();
            return dispatcherResult.Operation.Result;
        }

        public object Invoke(Delegate method, object[] args)
        {
            return _dispatcher.Invoke(method, DispatcherPriority.Normal, args);
        }

        private class DispatcherAsyncResult : IAsyncResult
        {
            private readonly IAsyncResult result;

            public DispatcherAsyncResult(DispatcherOperation operation)
            {
                this.Operation = operation;
                this.result = operation.Task;
            }

            public DispatcherOperation Operation { get; }

            public bool IsCompleted => this.result.IsCompleted;
            public WaitHandle AsyncWaitHandle => this.result.AsyncWaitHandle;
            public object AsyncState => this.result.AsyncState;
            public bool CompletedSynchronously => this.result.CompletedSynchronously;
        }
    }
}
