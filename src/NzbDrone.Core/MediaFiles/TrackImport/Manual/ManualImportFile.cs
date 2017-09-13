using System.Collections.Generic;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.TrackImport.Manual
{
    public class ManualImportFile
    {
        public string Path { get; set; }
        public int ArtistId { get; set; }
        public int AlbumId { get; set; }
        public List<int> TrackIds { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadId { get; set; }
    }
}
