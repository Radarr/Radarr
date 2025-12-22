using NzbDrone.Common.Messaging;
using NzbDrone.Core.Authors;

namespace NzbDrone.Core.Monitoring.Events
{
    public class AuthorMonitoringChangedEvent : IEvent
    {
        public Author Author { get; private set; }
        public bool PreviousMonitored { get; private set; }
        public int AffectedSeriesCount { get; set; }
        public int AffectedBooksCount { get; set; }
        public int AffectedAudiobooksCount { get; set; }

        public AuthorMonitoringChangedEvent(Author author, bool previousMonitored)
        {
            Author = author;
            PreviousMonitored = previousMonitored;
        }
    }
}
