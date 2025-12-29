using NzbDrone.Common.Messaging;

namespace NzbDrone.Core.Monitoring.Events
{
    public class BookSeriesMonitoringChangedEvent : IEvent
    {
        public NzbDrone.Core.BookSeries.BookSeries BookSeries { get; private set; }
        public bool PreviousMonitored { get; private set; }
        public int AffectedBooksCount { get; set; }
        public int AffectedAudiobooksCount { get; set; }

        public BookSeriesMonitoringChangedEvent(NzbDrone.Core.BookSeries.BookSeries bookSeries, bool previousMonitored)
        {
            BookSeries = bookSeries;
            PreviousMonitored = previousMonitored;
        }
    }
}
