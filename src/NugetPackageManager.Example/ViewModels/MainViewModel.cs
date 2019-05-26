using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using NugetPackageManager.Xaml.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NugetPackageManager.Example.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(IUIVisualizerService service)
        {
            uIVisualizerService = service;
        }

        protected override Task InitializeAsync()
        {
            InitializeCommands();
            return base.InitializeAsync();
        }

        public TaskCommand RunNuget { get; set; }

        private void InitializeCommands()
        {
            RunNuget = new TaskCommand(OnRunNuget);
        }

        private async Task OnRunNuget()
        {
            var nugetSettingsVm = this.GetTypeFactory().CreateInstanceWithParametersAndAutoCompletion<SettingsViewModel>();

            if(nugetSettingsVm !=null)
            {
                await uIVisualizerService.ShowDialogAsync(nugetSettingsVm);
            }
            
        }

        private readonly IUIVisualizerService uIVisualizerService;
    }
}
