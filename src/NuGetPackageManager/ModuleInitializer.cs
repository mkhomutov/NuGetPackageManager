using Catel.Configuration;
using Catel.IoC;
using NuGet.Credentials;
using NuGet.Frameworks;
using NuGet.Protocol.Core.Types;
using NuGetPackageManager.Cache;
using NuGetPackageManager.Management;
using NuGetPackageManager.Models;
using NuGetPackageManager.Providers;
using NuGetPackageManager.Services;
using NuGetPackageManager.Windows;
using NuGetPackageManager.Windows.Service;
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

        serviceLocator.RegisterType<IModelProvider<ExplorerSettingsContainer>, ExplorerSettingsContainerModelProvider>();

        serviceLocator.RegisterType<INuGetFeedVerificationService, NuGetFeedVerificationService>();

        serviceLocator.RegisterType<ICredentialProvider, WindowsCredentialProvider>();
        serviceLocator.RegisterType<ICredentialProviderLoaderService, CredentialProviderLoaderService>();

        serviceLocator.RegisterType<IPackageInstallationService, PackageInstallationService>();

        var appCache = new ApplcationCacheProvider();

        serviceLocator.RegisterInstance<IApplicationCacheProvider>(appCache);
        serviceLocator.RegisterType<IPackageMetadataMediaDownloadService, PackageMetadataMediaDownloadService>();

        serviceLocator.RegisterType<ISourceRepositoryProvider, SourceRepositoryProvider>();
        serviceLocator.RegisterType<INuGetProjectContextProvider, NuGetProjectContextProvider>();

        serviceLocator.RegisterType<IRepositoryService, RepositoryService>();

        serviceLocator.RegisterType<IExtensibleProjectLocator, ExtensibleProjectLocator>();
        serviceLocator.RegisterType<INuGetExtensibleProjectManager, NuGetExtensibleProjectManager>();

        serviceLocator.RegisterType<IFrameworkNameProvider, DefaultFrameworkNameProvider>();

        serviceLocator.RegisterType<ISynchronousUiVisualizer, SynchronousUIVisualizerService>();
        serviceLocator.RegisterType<IMessageDialogService, MessageDialogService>();

        serviceLocator.RegisterType<IFileDirectoryService, FileDirectoryService>();

        serviceLocator.RegisterType<INuGetCacheManager, NuGetCacheManager>();

        serviceLocator.RegisterType<IAnimationService, AnimationService>();

        serviceLocator.RegisterType<IProgressManager, ProgressManager>();

        //package loaders
        serviceLocator.RegisterType<IPackagesLoaderService, PackagesLoaderService>();
        //todo use separate providers instead of tags
        serviceLocator.RegisterTypeWithTag<IPackagesLoaderService, LocalPackagesLoaderService>("Installed");
        serviceLocator.RegisterTypeWithTag<IPackagesLoaderService, UpdatePackagesLoaderService>("Updates");

        serviceLocator.RegisterType<IDefferedPackageLoaderService, DefferedPackageLoaderService>();

        //add all project extensions

        var manager = serviceLocator.ResolveType<IExtensibleProjectLocator>();

        var directoryService = serviceLocator.ResolveType<IFileDirectoryService>();

        manager.Register<ExampleFolderPackageManagement>(directoryService.GetApplicationRoamingFolder());
        manager.Register<ExamplePackageManagement>(directoryService.GetApplicationRoamingFolder());
    }
}
