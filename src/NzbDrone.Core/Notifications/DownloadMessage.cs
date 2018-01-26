using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.Notifications
{
    public class DownloadMessage
    {
        public string Message { get; set; }
        public Movie Movie { get; set; }
        public MovieFile MovieFile { get; set; }
        public List<MovieFile> OldMovieFiles { get; set; }
        public string SourcePath { get; set; }
        public string DownloadId { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
