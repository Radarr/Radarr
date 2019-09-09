using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
