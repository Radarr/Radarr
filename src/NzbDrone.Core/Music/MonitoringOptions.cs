using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class MonitoringOptions : IEmbeddedDocument
    {
        public bool IgnoreTracksWithFiles { get; set; }
        public bool IgnoreTracksWithoutFiles { get; set; }
        public bool Monitored { get; set; }
    }
}
