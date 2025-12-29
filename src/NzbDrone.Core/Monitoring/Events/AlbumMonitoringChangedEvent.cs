using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Monitoring.Events
{
    public class AlbumMonitoringChangedEvent : IEvent
    {
        public Album Album { get; private set; }
        public bool PreviousMonitored { get; private set; }
        public int AffectedTracksCount { get; set; }

        public AlbumMonitoringChangedEvent(Album album, bool previousMonitored)
        {
            Album = album;
            PreviousMonitored = previousMonitored;
        }
    }
}
