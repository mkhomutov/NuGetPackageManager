namespace NuGetPackageManager
{
    using Catel.Logging;
    using System.Windows;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LogManager.AddDebugListener();

            base.OnStartup(e);
        }
    }
}
