using NzbDrone.Core.Datastore;
using System.Collections.Generic;

namespace NzbDrone.Core.Music
{
    public class MonitoringOptions : IEmbeddedDocument
    {
        public MonitoringOptions()
        {
            AlbumsToMonitor = new List<string>();
        }

        public bool IgnoreAlbumsWithFiles { get; set; }
        public bool IgnoreAlbumsWithoutFiles { get; set; }
        public List<string> AlbumsToMonitor { get; set; }
        public bool Monitored { get; set; }
    }
}
