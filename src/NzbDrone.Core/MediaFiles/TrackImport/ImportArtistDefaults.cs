using System.Collections.Generic;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public class ImportArtistDefaults
    {
        public int MetadataProfileId { get; set; }
        public int LanguageProfileId { get; set; }
        public int QualityProfileId { get; set; }
        public bool AlbumFolder { get; set; }
        public MonitorTypes Monitored { get; set; }
        public HashSet<int> Tags { get; set; }
    }
}
