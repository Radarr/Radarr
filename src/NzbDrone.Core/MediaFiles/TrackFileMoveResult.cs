using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class TrackFileMoveResult
    {
        public TrackFileMoveResult()
        {
            OldFiles = new List<BookFile>();
        }

        public BookFile TrackFile { get; set; }
        public List<BookFile> OldFiles { get; set; }
    }
}
