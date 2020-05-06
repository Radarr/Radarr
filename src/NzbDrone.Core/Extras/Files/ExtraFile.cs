using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Extras.Files
{
    public abstract class ExtraFile : ModelBase
    {
        public int AuthorId { get; set; }
        public int? TrackFileId { get; set; }
        public int? BookId { get; set; }
        public string RelativePath { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Extension { get; set; }
    }
}
