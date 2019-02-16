using Marr.Data;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public class TrackFile : ModelBase
    {
        // these are model properties
        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public Language Language { get; set; }
        public int AlbumId { get; set; }
        
        // These are queried from the database
        public LazyLoaded<List<Track>> Tracks { get; set; }
        public LazyLoaded<Artist> Artist { get; set; }
        public LazyLoaded<Album> Album { get; set; }

        public override string ToString()
        {
            return string.Format("[{0}] {1}", Id, RelativePath);
        }
    }
}
