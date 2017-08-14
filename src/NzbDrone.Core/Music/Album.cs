using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class Album : ModelBase
    {
        public Album()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public const string RELEASE_DATE_FORMAT = "yyyy-MM-dd";

        public string ForeignAlbumId { get; set; }
        public int ArtistId { get; set; }
        public string Title { get; set; }
        public string CleanTitle { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string Label { get; set; }
        //public int TrackCount { get; set; }
        public string Path { get; set; }
        public int ProfileId { get; set; }
        public int Duration { get; set; }
        public List<Track> Tracks { get; set; }
        //public int DiscCount { get; set; }
        public bool Monitored { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        //public List<Actor> Actors { get; set; } // TODO: These are band members. TODO: Refactor
        public List<string> Genres { get; set; }
        public DateTime? LastInfoSync { get; set; }
        public DateTime? LastDiskSync { get; set; }
        public DateTime Added { get; set; }
        public String AlbumType { get; set; } // TODO: Turn this into a type similar to Series Type in TV
        //public string ArtworkUrl { get; set; }
        //public string Explicitness { get; set; }
        public AddSeriesOptions AddOptions { get; set; }
        public Artist Artist { get; set; }
        public Ratings Ratings { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}][{1}]", ForeignAlbumId, Title.NullSafe());
        }
    }
}
