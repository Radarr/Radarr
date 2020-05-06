using System.Collections.Generic;

namespace NzbDrone.Core.MediaFiles
{
    public class RenameTrackFilePreview
    {
        public int AuthorId { get; set; }
        public int BookId { get; set; }
        public List<int> TrackNumbers { get; set; }
        public int TrackFileId { get; set; }
        public string ExistingPath { get; set; }
        public string NewPath { get; set; }
    }
}
