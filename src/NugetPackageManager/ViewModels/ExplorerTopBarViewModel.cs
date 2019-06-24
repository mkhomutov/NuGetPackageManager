using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NuGetPackageManager.ViewModels
{
    public class ExplorerTopBarViewModel : ViewModelBase
    {
        private readonly ITypeFactory _typeFactory;
        private readonly IUIVisualizerService _uIVisualizerService;

        public ExplorerTopBarViewModel(ITypeFactory typeFactory, IUIVisualizerService uIVisualizerService)
        {
            Argument.IsNotNull(() => typeFactory);
            Argument.IsNotNull(() => uIVisualizerService);

            _typeFactory = typeFactory;
            _uIVisualizerService = uIVisualizerService;

            Title = "Nuget - Solution";
            InitializeCommands();
        }

        protected override Task InitializeAsync()
        {
            return base.InitializeAsync();
        }

        private void InitializeCommands()
        {
            ShowPackageSourceSettings = new TaskCommand(OnShowPackageSourceSettingsExecuteAsync);
        }


        public TaskCommand ShowPackageSourceSettings { get; set; }

        private async Task OnShowPackageSourceSettingsExecuteAsync()
        {
            var nugetSettingsVm = _typeFactory.CreateInstanceWithParametersAndAutoCompletion<SettingsViewModel>();

            if (nugetSettingsVm != null)
            {
                await _uIVisualizerService.ShowDialogAsync(nugetSettingsVm);
            }
        }

    }
}
