using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalTrack
    {
        public LocalTrack()
        {
            Tracks = new List<Track>();
        }

        public string Path { get; set; }
        public long Size { get; set; }
        public ParsedTrackInfo ParsedTrackInfo { get; set; }
        public Artist Artist { get; set; }
        public List<Track> Tracks { get; set; }
        public QualityModel Quality { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public bool ExistingFile { get; set; }

        public string Album
        {
            get
            {
                return Tracks.Select(c => c.AlbumId).Distinct().Single();
            }
        }

        public bool IsSpecial => Album != "";

        public override string ToString()
        {
            return Path;
        }
    }
}
