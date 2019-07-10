namespace NuGetPackageManager.Pagination
{
    using Catel.Logging;
    using NuGet.Configuration;

    public class PageContinuation
    {
        int _lastNumber = -1;

        int _pageSize = 1;

        int startNumber = 1;

        static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public PageContinuation(int pageSize, string source)
        {
            _pageSize = pageSize;
            startNumber = _lastNumber - pageSize;    //first GetNext() returns zero position
            _lastNumber = startNumber;

            Source = new PackageSource(source);
        }

        public int LastNumber { get => _lastNumber; private set => _lastNumber = value; }

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
            _lastNumber = startNumber;
        }
    }
}
