namespace NuGetPackageManager.Models
{
    using Catel.Data;
    using System.Collections.Generic;

    public class NuGetActionTarget : ModelBase
    {
        private List<IExtensibleProject> extensibleProjects = new List<IExtensibleProject>();

        public IReadOnlyList<IExtensibleProject> TargetProjects => extensibleProjects;

        public void Add(IExtensibleProject project)
        {
            extensibleProjects.Add(project);
        }

        public void Remove(IExtensibleProject project)
        {
            extensibleProjects.Remove(project);
        }
    }
}
