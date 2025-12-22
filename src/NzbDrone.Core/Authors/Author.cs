using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Authors
{
    public class Author : ModelBase
    {
        public Author()
        {
            Tags = new HashSet<int>();
        }

        public string Name { get; set; }
        public string SortName { get; set; }
        public string Description { get; set; }
        public string ForeignAuthorId { get; set; }
        public bool Monitored { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public int QualityProfileId { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
