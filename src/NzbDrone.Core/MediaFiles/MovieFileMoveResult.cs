using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class MovieFileMoveResult
    {
        public MovieFileMoveResult()
        {
            OldFiles = new List<MovieFile>();
        }

        public MovieFile MovieFile { get; set; }
        public List<MovieFile> OldFiles { get; set; }
    }
}
