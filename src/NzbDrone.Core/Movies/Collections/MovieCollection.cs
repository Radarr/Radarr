using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Movies.Collections
{
    public class MovieCollection : ModelBase
    {
        public MovieCollection()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public string SortTitle { get; set; }
        public int TmdbId { get; set; }
        public string Overview { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public string RootFolderPath { get; set; }
        public bool SearchOnAdd { get; set; }
        public MovieStatusType MinimumAvailability { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public DateTime Added { get; set; }
        public List<MovieMetadata> Movies { get; set; }

        public void ApplyChanges(MovieCollection otherCollection)
        {
            TmdbId = otherCollection.TmdbId;

            Monitored = otherCollection.Monitored;
            SearchOnAdd = otherCollection.SearchOnAdd;
            QualityProfileId = otherCollection.QualityProfileId;
            MinimumAvailability = otherCollection.MinimumAvailability;
            RootFolderPath = otherCollection.RootFolderPath;
        }
    }
}
