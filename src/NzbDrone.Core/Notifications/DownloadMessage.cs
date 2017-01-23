using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class DownloadMessage
    {
        public string Message { get; set; }
        public Series Series { get; set; }
        public Movie Movie { get; set; }
        public EpisodeFile EpisodeFile { get; set; }
        public List<EpisodeFile> OldFiles { get; set; }
        public MovieFile MovieFile { get; set; }
        public List<MovieFile> OldMovieFiles { get; set; }
        public string SourcePath { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
