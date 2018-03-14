using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.MovieImport.Manual
{
    public class ManualImportItem
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
        public Movie Movie { get; set; }
    }
}
