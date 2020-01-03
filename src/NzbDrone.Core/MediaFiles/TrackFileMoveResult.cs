using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class TrackFileMoveResult
    {
        public TrackFileMoveResult()
        {
            OldFiles = new List<TrackFile>();
        }

        public TrackFile TrackFile { get; set; }
        public List<TrackFile> OldFiles { get; set; }
    }
}
