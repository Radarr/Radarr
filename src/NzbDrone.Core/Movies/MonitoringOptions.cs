using NzbDrone.Core.Datastore;

namespace NzbDrone.Core.Movies
{
    public class MonitoringOptions : IEmbeddedDocument
    {
        public bool IgnoreEpisodesWithFiles { get; set; }
        public bool IgnoreEpisodesWithoutFiles { get; set; }
    }
}
