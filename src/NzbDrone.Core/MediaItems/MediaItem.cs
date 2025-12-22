using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaTypes;

namespace NzbDrone.Core.MediaItems
{
    public abstract class MediaItem : ModelBase
    {
        protected MediaItem()
        {
            Tags = new HashSet<int>();
        }

        public MediaType MediaType { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public string RootFolderPath { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public int? AuthorId { get; set; }
        public int? SeriesId { get; set; }

        public abstract string GetTitle();
        public abstract int GetYear();
    }
}
