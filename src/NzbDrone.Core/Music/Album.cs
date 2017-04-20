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

        public int AlbumId { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
        public int TrackCount { get; set; }
        public int DiscCount { get; set; }
        public bool Monitored { get; set; }
        public List<MediaCover.MediaCover> Images { get; set; }
        public List<Actor> Actors { get; set; } // These are band members. TODO: Refactor
    }
}
