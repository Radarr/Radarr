using NzbDrone.Common.Messaging;
using NzbDrone.Core.TV;

namespace NzbDrone.Core.Monitoring.Events
{
    public class TVShowMonitoringChangedEvent : IEvent
    {
        public TVShow TVShow { get; private set; }
        public bool PreviousMonitored { get; private set; }
        public int AffectedSeasonsCount { get; set; }
        public int AffectedEpisodesCount { get; set; }

        public TVShowMonitoringChangedEvent(TVShow tvShow, bool previousMonitored)
        {
            TVShow = tvShow;
            PreviousMonitored = previousMonitored;
        }
    }
}
