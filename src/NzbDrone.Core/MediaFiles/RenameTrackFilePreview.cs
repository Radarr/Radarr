using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class RenameTrackFilePreview
    {
        public int ArtistId { get; set; }
        public int AlbumId { get; set; }
        public List<int> TrackNumbers { get; set; }
        public int TrackFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }
}
