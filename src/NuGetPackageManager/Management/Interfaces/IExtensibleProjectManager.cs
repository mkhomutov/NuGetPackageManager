﻿namespace NuGetPackageManager.Management
{
    using System.Collections.Generic;

    public interface IExtensibleProjectManager
    {
        IEnumerable<IExtensibleProject> GetAllExtensibleProjects();

        void Register(IExtensibleProject project);

        void Register<T>() where T : IExtensibleProject;

        void Register<T>(object[] parameters) where T : IExtensibleProject;

        void Enable(IExtensibleProject extensibleProject);

        void Disable(IExtensibleProject extensibleProject);

        bool IsEnabled(IExtensibleProject extensibleProject);

        bool IsConfigLoaded { get; }

        void PersistChanges();

        void RestoreStateFromConfig();
    }
}