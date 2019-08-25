using Catel.Configuration;
using Catel.IoC;
using NuGet.Credentials;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Models;
using NuGetPackageManager.Providers;
using NuGetPackageManager.Services;
using SourceRepositoryProvider = NuGetPackageManager.Providers.SourceRepositoryProvider;

/// <summary>
/// Used by the ModuleInit. All code inside the Initialize method is ran as soon as the assembly is loaded.
/// </summary>
public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the module.
    /// </summary>
    public static void Initialize()
    {
        var serviceLocator = ServiceLocator.Default;

        serviceLocator.RegisterType<IConfigurationService, NugetConfigurationService>();
        serviceLocator.RegisterType<IModelProvider<NuGetFeed>, ModelProvider<NuGetFeed>>();

        var settingsProvider = new ModelProvider<ExplorerSettingsContainer>();
        settingsProvider.Model = new ExplorerSettingsContainer();
        serviceLocator.RegisterInstance<IModelProvider<ExplorerSettingsContainer>>(settingsProvider);

        serviceLocator.RegisterType<INuGetFeedVerificationService, NuGetFeedVerificationService>();

        serviceLocator.RegisterType<ICredentialProvider, WindowsCredentialProvider>();
        serviceLocator.RegisterType<ICredentialProviderLoaderService, CredentialProviderLoaderService>();

        serviceLocator.RegisterType<IPackagesLoaderService, PackagesLoaderService>();

        var appCache = new ApplcationCacheProvider();

        serviceLocator.RegisterInstance<IApplicationCacheProvider>(appCache);
        serviceLocator.RegisterType<IPackageMetadataMediaDownloadService, PackageMetadataMediaDownloadService>();



        serviceLocator.RegisterType<ISourceRepositoryProvider, SourceRepositoryProvider>();
        //serviceLocator.RegisterInstance<ISourceRepositoryProvider>();

        serviceLocator.RegisterType<IRepositoryService, RepositoryService>();
    }
}
