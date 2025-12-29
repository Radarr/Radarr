using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Music
{
    public class MusicFile : ModelBase
    {
        public int? TrackId { get; set; }
        public int? AlbumId { get; set; }
        public string RelativePath { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }
        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }
        public QualityModel Quality { get; set; }
        public string AudioFormat { get; set; }
        public int? Bitrate { get; set; }
        public int? SampleRate { get; set; }
        public int? Channels { get; set; }

        public override string ToString()
        {
            return RelativePath;
        }
    }
}
