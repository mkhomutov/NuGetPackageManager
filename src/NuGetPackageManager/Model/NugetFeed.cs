using Catel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Model
{
    public class NugetFeed : ModelBase
    {
        public NugetFeed()
        {

        }

        public NugetFeed(string name, string source)
        {
            Name = name;
            Source = source;
        }

        public string Name { get; set; }

        public string Source { get; set; }

        public bool IsActive { get; set; }

        public override string ToString()
        {
            return $"{Name}\n{Source}";
        }

        public void ForceCancelEdit()
        {
            IEditableObject eo = this;
            eo.CancelEdit();
        }
    }
}
