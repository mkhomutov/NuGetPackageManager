namespace NuGetPackageManager.Services
{
    using NuGetPackageManager.Pagination;
    using System.Threading.Tasks;

    public interface IDefferedPackageLoaderService
    {
        void Add(DeferToken token);

        Task StartLoadingAsync();
    }
}
