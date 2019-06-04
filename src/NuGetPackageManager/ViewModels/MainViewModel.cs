using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IUIVisualizerService _uIVisualizerService;
        private readonly ITypeFactory _typeFactory;

        public MainViewModel(ITypeFactory typeFactory, IUIVisualizerService service)
        {
            Argument.IsNotNull(() => service);
            Argument.IsNotNull(() => typeFactory);

            _uIVisualizerService = service;
            _typeFactory = typeFactory;

            InitializeCommands();
        }

        public TaskCommand RunNuget { get; set; }

        private void InitializeCommands()
        {
            RunNuget = new TaskCommand(OnRunNugetExecute);
        }

        private async Task OnRunNugetExecute()
        {
            var nugetSettingsVm = _typeFactory.CreateInstanceWithParametersAndAutoCompletion<SettingsViewModel>();

            if(nugetSettingsVm !=null)
            {
                await _uIVisualizerService.ShowDialogAsync(nugetSettingsVm);
            }
            
        }
    }
}
