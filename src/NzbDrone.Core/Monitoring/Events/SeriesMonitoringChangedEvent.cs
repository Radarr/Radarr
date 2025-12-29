using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Monitoring.Events
{
    public class SeriesMonitoringChangedEvent : IEvent
    {
        public NzbDrone.Core.Series.Series Series { get; private set; }
        public bool PreviousMonitored { get; private set; }
        public int AffectedBooksCount { get; set; }
        public int AffectedAudiobooksCount { get; set; }

        public SeriesMonitoringChangedEvent(NzbDrone.Core.Series.Series series, bool previousMonitored)
        {
            Series = series;
            PreviousMonitored = previousMonitored;
        }
    }
}
