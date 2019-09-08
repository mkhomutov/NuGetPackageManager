using NuGetPackageManager.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Services
{
    public interface IMessageDialogService
    {
        Task<T> ShowDialogAsync<T>(string title, string message, bool addCloseButton, params IDialogOption[] options);

        T ShowDialog<T>(string title, string message, bool addCloseButton, params IDialogOption[] options);
    }
}
