using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NzbDrone.Core.Languages;

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
        public Album Album { get; set; }
        public AlbumRelease Release { get; set; }
        public List<Track> Tracks { get; set; }
        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public bool ExistingFile { get; set; }


        public override string ToString()
        {
            return Path;
        }
    }
}
