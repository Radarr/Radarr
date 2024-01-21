using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class MovieFileMoveResult
    {
        public MovieFileMoveResult()
        {
            OldFiles = new List<DeletedMovieFile>();
        }

        public MovieFile MovieFile { get; set; }
        public List<DeletedMovieFile> OldFiles { get; set; }
    }
}
