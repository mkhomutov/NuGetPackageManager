using Catel.Logging;
using NuGet.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Pagination
{
    public class PageContinuation
    {
        int _lastNumber = -1;
        int _pageSize = 1;

        static readonly ILog Log = LogManager.GetCurrentClassLogger();
        
        public PageContinuation(int pageSize, string source)
        {
            _pageSize = pageSize;
            _lastNumber = _lastNumber - pageSize;    //first GetNext() returns zero position
            Source = new PackageSource(source);
        }

        public int LastNumber
        {
            get => _lastNumber;
            private set => _lastNumber = value;
        }

        public int Size => _pageSize;

        public int Next => LastNumber + 1;

        public PackageSource Source { get; private set; }

        public int GetNext()
        {
            LastNumber = LastNumber + Size;

            Log.Info($"Got next {Size} positions, starts from {Next}");

            return Next;
        }

        public void Reset()
        {
            _lastNumber = -1;
        }
    }
}
