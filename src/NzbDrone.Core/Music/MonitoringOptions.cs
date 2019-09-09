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
        
        public MonitorTypes Monitor { get; set; }
        public List<string> AlbumsToMonitor { get; set; }
        public bool Monitored { get; set; }
    }

    public enum MonitorTypes
    {
        All,
        Future,
        Missing,
        Existing,
        Latest,
        First,
        None,
        Unknown
    }
}
