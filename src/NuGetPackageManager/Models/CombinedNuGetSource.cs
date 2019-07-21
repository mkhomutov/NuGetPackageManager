﻿using Catel.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuGetPackageManager.Models
{
    public class CombinedNuGetSource : INuGetSource
    {
        private List<string> sourceList = new List<string>();

        public string Name => "All";
        public string Source { get; set; }

        public bool MultipleSource => true;

        public CombinedNuGetSource(IEnumerable<INuGetSource> feedList)
        {
            feedList.ForEach(x => sourceList.Add(x.Source));
            Source = feedList.FirstOrDefault().Source;
        }

        public void AddFeed(NuGetFeed source)
        {
            sourceList.Add(source.Source);
        }

        public IEnumerable<string> GetAllSources()
        {
            return sourceList;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
