using System;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.TV
{
    public class EpisodeFile : ModelBase
    {
        public int TVShowId { get; set; }
        public int SeasonNumber { get; set; }

        public string RelativePath { get; set; }
        public string Path { get; set; }
        public long Size { get; set; }
        public DateTime DateAdded { get; set; }

        public string SceneName { get; set; }
        public string ReleaseGroup { get; set; }

        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public StreamingSource StreamingSource { get; set; }

        public string MediaInfo { get; set; }

        public override string ToString()
        {
            return Path;
        }
    }
}
