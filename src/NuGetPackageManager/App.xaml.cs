using Catel.IoC;
using Catel.Logging;
using Catel.MVVM;
using NuGetPackageManager;
using NuGetPackageManager.ViewModels;
using NuGetPackageManager.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace NuGetPackageManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            LogManager.AddDebugListener();

            var slocator = this.GetServiceLocator();
            var vmlocator = slocator.ResolveType<IViewModelLocator>();

            vmlocator.Register<MainWindow, MainViewModel>();
            vmlocator.Register<SettingsControlView, SettingsControlViewModel>();

            base.OnStartup(e);
        }
    }
}
