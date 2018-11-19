using System.Collections.Generic;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Music;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.MediaFiles.TrackImport.Manual
{
    public class ManualImportItem : ModelBase
    {
        public string Path { get; set; }
        public string RelativePath { get; set; }
        public string FolderName { get; set; }
        public string Name { get; set; }
        public long Size { get; set; }
        public Artist Artist { get; set; }
        public Album Album { get; set; }
        public List<Track> Tracks { get; set; }
        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public string DownloadId { get; set; }
        public IEnumerable<Rejection> Rejections { get; set; }
    }
}
