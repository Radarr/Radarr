using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles
{
    public class TrackFile : ModelBase
    {
        //public string ForeignTrackId { get; set; }
        //public string ForeignArtistId { get; set; }
        public int AlbumId { get; set; }
        public int ArtistId { get; set; }
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        //public LazyLoaded<List<Track>> Episodes { get; set; }
        public LazyLoaded<Artist> Artist { get; set; }
        public LazyLoaded<List<Track>> Tracks { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Id, RelativePath);
        }
    }
}
