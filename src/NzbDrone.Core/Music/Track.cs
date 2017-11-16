using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using Marr.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public class Track : ModelBase
    {
        public Track()
        {
        }

        public const string RELEASE_DATE_FORMAT = "yyyy-MM-dd";

        public string ForeignTrackId { get; set; }
        public int AlbumId { get; set; }
        public Artist Artist { get; set; }
       
        public int ArtistId { get; set; } // This is the DB Id of the Artist, not the SpotifyId
        //public int CompilationId { get; set; }
        public bool Compilation { get; set; }
        public string TrackNumber { get; set; }
        public int AbsoluteTrackNumber { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
        //public bool Ignored { get; set; }
        public bool Explicit { get; set; }
        public bool Monitored { get; set; }
        public int TrackFileId { get; set; } 
        public Ratings Ratings { get; set; }
        public int MediumNumber { get; set; }
        //public DateTime? ReleaseDate { get; set; }

        public LazyLoaded<TrackFile> TrackFile { get; set; }

        public Album Album { get; set; }

        public bool HasFile => TrackFileId > 0;

        public override string ToString()
        {
            return string.Format("[{0}]{1}", ForeignTrackId, Title.NullSafe());
        }
    }
}
