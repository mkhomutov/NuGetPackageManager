using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management
{
    public interface IExtensibleProjectManager
    {
        IEnumerable<IExtensibleProject> GetAllExtensibleProjects();

        void Register(IExtensibleProject project);

        void Register<T>() where T : IExtensibleProject;

        void Register<T>(object[] parameters) where T : IExtensibleProject;

        void Enable(IExtensibleProject extensibleProject);
        void Disable(IExtensibleProject extensibleProject);

        bool IsEnabled(IExtensibleProject extensibleProject);

        void PersistChanges();
        void RestoreStateFromConfig();
    }
}
