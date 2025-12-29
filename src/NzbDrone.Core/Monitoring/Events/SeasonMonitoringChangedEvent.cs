using NzbDrone.Common.Messaging;
using NzbDrone.Core.TV;

namespace NzbDrone.Core.Monitoring.Events
{
    public class SeasonMonitoringChangedEvent : IEvent
    {
        public Season Season { get; private set; }
        public bool PreviousMonitored { get; private set; }
        public int AffectedEpisodesCount { get; set; }

        public SeasonMonitoringChangedEvent(Season season, bool previousMonitored)
        {
            Season = season;
            PreviousMonitored = previousMonitored;
        }
    }
}
