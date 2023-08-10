using System;
using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Extras.Files
{
    public abstract class ExtraFile : ModelBase
    {
        public int MovieId { get; set; }
        public int? MovieFileId { get; set; }
        public string RelativePath { get; set; }
        public DateTime Added { get; set; }
        public DateTime LastUpdated { get; set; }
        public string Extension { get; set; }

        public override string ToString()
        {
            return $"[{Id}] {RelativePath}";
        }
    }

    public enum ExtraFileType
    {
        Subtitle = 0,
        Metadata = 1,
        Other = 2
    }
}
