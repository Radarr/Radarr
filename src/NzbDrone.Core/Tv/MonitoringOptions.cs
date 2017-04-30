using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Tv
{
    public class MonitoringOptions : IEmbeddedDocument
    {
        public bool IgnoreEpisodesWithFiles { get; set; }
        public bool IgnoreEpisodesWithoutFiles { get; set; }

        public bool IgnoreTracksWithFiles { get; set; }
        public bool IgnoreTracksWithoutFiles { get; set; }
    }
}
