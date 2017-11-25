using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Music
{
    public class MonitoringOptions : IEmbeddedDocument
    {
        public bool IgnoreAlbumsWithFiles { get; set; }
        public bool IgnoreAlbumsWithoutFiles { get; set; }
        public bool Monitored { get; set; }
    }
}
