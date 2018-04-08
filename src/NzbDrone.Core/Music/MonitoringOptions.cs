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
        
        public MonitoringOption SelectedOption { get; set; }
        public List<string> AlbumsToMonitor { get; set; }
        public bool Monitored { get; set; }
    }

    public enum MonitoringOption
    {
        All = 0,
        Future = 1,
        Missing = 2,
        Existing = 3,
        Latest = 4,
        First = 5,
        None = 6
    }
}
