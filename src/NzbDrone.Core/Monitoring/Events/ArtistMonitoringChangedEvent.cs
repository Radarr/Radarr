using NzbDrone.Common.Messaging;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Monitoring.Events
{
    public class ArtistMonitoringChangedEvent : IEvent
    {
        public Artist Artist { get; private set; }
        public bool PreviousMonitored { get; private set; }
        public int AffectedAlbumsCount { get; set; }
        public int AffectedTracksCount { get; set; }

        public ArtistMonitoringChangedEvent(Artist artist, bool previousMonitored)
        {
            Artist = artist;
            PreviousMonitored = previousMonitored;
        }
    }
}
