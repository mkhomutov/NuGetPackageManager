namespace NuGetPackageManager.Models
{
    using Catel.Data;
    using Catel.Logging;
    using System;
    using System.ComponentModel;

    public class NuGetFeed : ModelBase, ICloneable<NuGetFeed>, IDataErrorInfo
    {
        public NuGetFeed()
        {
            VerificationResult = FeedVerificationResult.Unknown;
            Error = String.Empty;
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

        public FeedVerificationResult VerificationResult { get; set; }

        public bool IsNameValid { get; private set; }

        public bool IsAccessible { get; set; }

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
            {
                IsActive = this.IsActive,
                VerificationResult = this.VerificationResult
            };
        }

        public string Error { get; private set; }


        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Name):

                        if (!IsNameValid)
                        {
                            return "Feed name cannot be empty";
                        }
                        break;

                    case nameof(Source):

                        if (GetUriSource() == null)
                        {
                            return "Incorrect feed source can`t be recognized as Uri";
                        }
                        break;
                }

                return String.Empty;
            }
        }

        protected override void OnPropertyChanged(AdvancedPropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Source))
            {
                //reset verification
                VerificationResult = FeedVerificationResult.Unknown;
            }
            if(e.PropertyName == nameof(VerificationResult))
            {
                IsAccessible = VerificationResult == FeedVerificationResult.Valid;
            }
            if(e.PropertyName == nameof(Name))
            {
                IsNameValid =  !String.IsNullOrEmpty(Name);
            }
            base.OnPropertyChanged(e);
        }

        private static readonly ILog log = LogManager.GetCurrentClassLogger();
    }
}
