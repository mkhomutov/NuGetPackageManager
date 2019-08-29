using Catel;
using Catel.IoC;
using Catel.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Management
{
    public class ExtensibleProjectManager : IExtensibleProjectManager
    {
        private readonly static ILog Log = LogManager.GetCurrentClassLogger();

        private readonly ITypeFactory _typeFactory;
        private readonly Dictionary<Type, IExtensibleProject> _registredProjects = new Dictionary<Type, IExtensibleProject>();
        private readonly HashSet<IExtensibleProject> _enabledProjects = new HashSet<IExtensibleProject>();

        public ExtensibleProjectManager(ITypeFactory typeFactory)
        {
            Argument.IsNotNull(() => typeFactory);
            _typeFactory = typeFactory;
        }

        public bool IsEnabled(IExtensibleProject extensibleProject)
        {
            return _enabledProjects.Contains(extensibleProject);
        }

        public void Enable(IExtensibleProject extensibleProject)
        {
            var registeredProject = _registredProjects[extensibleProject.GetType()];

            if(registeredProject != extensibleProject)
            {
                throw new InvalidOperationException("ExtensibleProject must be registered before use");
            }

            if(!_enabledProjects.Add(registeredProject))
            {
                Log.Info($"Project {extensibleProject} already enabled");
            }
        }

        public void Disable(IExtensibleProject extensibleProject)
        {
            var registeredProject = _registredProjects[extensibleProject.GetType()];

            if (registeredProject != extensibleProject)
            {
                throw new InvalidOperationException("ExtensibleProject must be registered before use");
            }

            if(!_enabledProjects.Remove(registeredProject))
            {
                Log.Info($"Attempt to disable Project {extensibleProject}, which is not enabled");
            }
        }

        public IEnumerable<IExtensibleProject> GetAllExtensibleProjects()
        {
            return _registredProjects.Values;
        }

        public void Register(IExtensibleProject project)
        {
            _registredProjects[project.GetType()] = project;
        }

        public void Register<T>() where T : IExtensibleProject
        {
            Register<T>(new object[] { });
        }

        public void Register<T>(object[] parameters) where T : IExtensibleProject
        {
            var instance =_typeFactory.CreateInstanceWithParametersAndAutoCompletion<T>(parameters);

            Register(instance);
        }
    }
}
