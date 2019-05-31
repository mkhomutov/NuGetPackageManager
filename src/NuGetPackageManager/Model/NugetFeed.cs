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
    public class NugetFeed : ModelBase, ICloneable
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

        public void ForceEndEdit()
        {
            IEditableObject eo = this;
            eo.EndEdit();
        }

        public object Clone()
        {
            return new NugetFeed(this.Name, this.Source) { IsActive = this.IsActive };
        }
    }
}
