using NzbDrone.Core.Datastore;
using NzbDrone.Core.Tv;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{
    public class Album : IEmbeddedDocument
    {
        public Album()
        {
            Images = new List<MediaCover.MediaCover>();
        }

        public string AlbumId { get; set; }
        public string Title { get; set; } // NOTE: This should be CollectionName in API
        public int Year { get; set; }
        public int TrackCount { get; set; }
        public List<Track> Tracks { get; set; }
        public int DiscCount { get; set; }
        public bool Monitored { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public List<Actor> Actors { get; set; } // These are band members. TODO: Refactor
        public List<string> Genres { get; set; }
        public string ArtworkUrl { get; set; }
        public string Explicitness { get; set; }
    }
}
