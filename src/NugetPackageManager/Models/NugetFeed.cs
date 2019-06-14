namespace NuGetPackageManager.Models
{
    using Catel.Data;
    using System;
    using System.ComponentModel;

    public class NuGetFeed : ModelBase, ICloneable<NuGetFeed>, IDataErrorInfo
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

        public bool IsVerifiedNow { get; set; }

        public int TestCount { get; set; }

        public FeedVerificationResult VerificationResult { get; set; } = FeedVerificationResult.Valid;

        public bool IsNameValid => !String.IsNullOrEmpty(Name);

        public bool IsAccessible => VerificationResult == FeedVerificationResult.Valid;

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
                Error = "Incorrect feed source can`t be recognized as Uri";
                return null;
            }
        }

        public NuGetFeed Clone()
        {
            return new NuGetFeed(
                this.Name, this.Source)
            { IsActive = this.IsActive };
        }

        public void CopyTo(NuGetFeed feed)
        {
            feed.VerificationResult = VerificationResult;
            feed.Source = Source;
            feed.Name = Name;
            feed.IsActive = IsActive;
        }

        public string Error { get; private set; } = String.Empty;


        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Name):
                        {
                            if (!IsNameValid)
                            {
                                return "Feed name cannot be empty";
                            }
                            break;
                        }
                    case nameof(Source):
                        {
                            if (GetUriSource() == null)
                            {
                                return "Incorrect feed source can`t be recognized as Uri";
                            }
                            break;
                        }
                }

                return String.Empty;
            }
        }
    }
}
