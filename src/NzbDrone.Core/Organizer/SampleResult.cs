using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Organizer
{
    public class SampleResult
    {
        public string FileName { get; set; }
        public Series Series { get; set; }
        public Artist Artist { get; set; }
        public Album Album { get; set; }
        public List<Episode> Episodes { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
        public List<Track> Tracks { get; set; }
        public TrackFile TrackFile { get; set; }
    }
}
