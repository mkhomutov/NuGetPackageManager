namespace NuGetPackageManager.Model
{
    using System.ComponentModel;
    using Catel.Data;

    public class NugetFeed : ModelBase, ICloneable<NugetFeed>
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

        public NugetFeed Clone()
        {
            return new NugetFeed(Name, Source) {IsActive = IsActive};
        }

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
    }
}