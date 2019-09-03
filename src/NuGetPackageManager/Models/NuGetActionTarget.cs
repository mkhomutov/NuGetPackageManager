using Catel.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
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
