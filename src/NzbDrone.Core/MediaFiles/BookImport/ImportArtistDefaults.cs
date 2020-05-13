using System.Collections.Generic;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.MediaFiles.BookImport
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
