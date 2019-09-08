using Catel.IoC;
using Catel.MVVM;
using Catel.Services;
using Catel.Windows;
using NuGetPackageManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Windows
{
    public class MessageDialogService : IMessageDialogService
    {
        private readonly IUIVisualizerService _uIVisualizerService;
        private readonly ITypeFactory _typeFactory;

        static MessageDialogService()
        {
            var vmLocator = ServiceLocator.Default.ResolveType<IViewModelLocator>();

            vmLocator.Register<DialogHost, DialogHostViewModel>();
             
        }

        public MessageDialogService(IUIVisualizerService uIVisualizerService, ITypeFactory typeFactory, IMessageService messageService)
        {
            _uIVisualizerService = uIVisualizerService;
            _typeFactory = typeFactory;
        }

        public async Task<T> ShowDialogAsync<T>(string title, string message, bool addCloseButton, params IDialogOption[] options)
        {
            var customize = new DialogCustomization(options, addCloseButton);

            var result = new DialogResult<T>();

            var vm = _typeFactory.CreateInstanceWithParametersAndAutoCompletion<DialogHostViewModel>(customize, result, title, message);

            await _uIVisualizerService.ShowDialogAsync(vm);

            return result.Result;
        }

        public T ShowDialog<T>(string title, string message, bool addCloseButton, params IDialogOption[] options)
        {
            var task = ShowDialogAsync<T>(title, message, addCloseButton, options);

            return default(T);
        }
    }
}
