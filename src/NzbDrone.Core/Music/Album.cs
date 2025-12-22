using System;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaTypes;

namespace NzbDrone.Core.Music
{
    public class Album : ModelBase
    {
        public Album()
        {
            Tags = new HashSet<int>();
            MediaType = MediaType.Music;
        }

        public int? ArtistId { get; set; }
        public string Title { get; set; }
        public string SortTitle { get; set; }
        public string Description { get; set; }
        public string ForeignAlbumId { get; set; }
        public string DiscogsId { get; set; }
        public MediaType MediaType { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string AlbumType { get; set; }
        public bool Monitored { get; set; }
        public int QualityProfileId { get; set; }
        public string Path { get; set; }
        public DateTime Added { get; set; }
        public HashSet<int> Tags { get; set; }
        public DateTime? LastSearchTime { get; set; }

        public override string ToString()
        {
            return $"{Title} ({ReleaseDate?.Year})";
        }
    }
}
