using System.Collections.Generic;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Manual
{
    public class ManualImportFile
    {
        public string Path { get; set; }
        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadId { get; set; }
        public int MovieId { get; set; }
    }
}
