namespace NuGetPackageManager.Pagination
{
    using Catel.Logging;
    using NuGet.Configuration;
    using System.Linq;

    public class PageContinuation
    {
        private int _lastNumber = -1;

        private int _pageSize = 1;

        private int _startNumber = 1;

        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public PageContinuation(int pageSize, params string[] sources)
        {
            _pageSize = pageSize;
            _startNumber = _lastNumber - pageSize;    //first GetNext() returns zero position
            _lastNumber = _startNumber;

            Source = sources.Select(s => new PackageSource(s)).ToArray();
        }

        public int LastNumber { get => _lastNumber; private set => _lastNumber = value; }

        public int Size => _pageSize;

        public int Next => LastNumber + 1;

        public PackageSource[] Source { get; private set; }

        public int GetNext()
        {
            LastNumber = LastNumber + Size;

            Log.Info($"Got next {Size} positions, starts from {Next}");

            return Next;
        }

        public void Reset()
        {
            _lastNumber = _startNumber;
        }


    }
}
