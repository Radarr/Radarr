using System.Collections.Generic;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Calibre;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.RootFolders
{
    public class RootFolder : ModelBase
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public int DefaultMetadataProfileId { get; set; }
        public int DefaultQualityProfileId { get; set; }
        public MonitorTypes DefaultMonitorOption { get; set; }
        public HashSet<int> DefaultTags { get; set; }
        public bool IsCalibreLibrary { get; set; }
        public CalibreSettings CalibreSettings { get; set; }

        public bool Accessible { get; set; }
        public long? FreeSpace { get; set; }
        public long? TotalSpace { get; set; }
    }
}
