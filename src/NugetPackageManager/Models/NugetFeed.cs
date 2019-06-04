using Catel.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public class NuGetFeed : ModelBase, ICloneable<NuGetFeed>
    {
        public NuGetFeed()
        {

        }

        public NuGetFeed(string name, string source)
        {
            Name = name;
            Source = source;
        }

        public string Name { get; set; }

        public string Source { get; set; }

        public bool IsActive { get; set; }

        protected bool IsNameValid => !String.IsNullOrEmpty(Name);

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

        public bool IsValid()
        {
            return IsNameValid && GetUriSource() != null;
        }

        public bool IsLocal()
        {
            return GetUriSource()?.IsLoopback ?? false;
        }


        public Uri GetUriSource()
        {
            try
            {
                return String.IsNullOrEmpty(Source) ? null : new Uri(Source);
            }
            catch (UriFormatException)
            {
                return null;
            }
        }


        public NuGetFeed Clone()
        {
            return new NuGetFeed(this.Name, this.Source) { IsActive = this.IsActive };
        }
    }
}
