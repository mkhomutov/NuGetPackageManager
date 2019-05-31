using Catel.Configuration;
using Catel.IoC;
using NugetPackageManager.Xaml.Providers;
using NugetPackageManager.Xaml.Services;
using NuGetPackageManager.Model;

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
        serviceLocator.RegisterType<IModelProvider<NugetFeed>, ModelProvider<NugetFeed>>();
    }
}