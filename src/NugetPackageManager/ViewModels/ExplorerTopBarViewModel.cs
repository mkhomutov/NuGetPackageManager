﻿using Catel;
using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using NuGetPackageManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<NuGetFeed> ActiveFeeds { get; set; } = new ObservableCollection<NuGetFeed>();


        #region commands
        public TaskCommand ShowPackageSourceSettings { get; set; }

        /*placeholder, this probably should be application command inside separate Catel command container*/
        public Command RefreshCurrentPage { get; set; }

        #endregion

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
