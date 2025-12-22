using System.Collections.Generic;
using NzbDrone.Core.Audiobooks;
using NzbDrone.Core.Books;

namespace NzbDrone.Core.Monitoring
{
    public interface IHierarchicalMonitoringService
    {
        bool IsEffectivelyMonitored(Book book);
        bool IsEffectivelyMonitored(Audiobook audiobook);
        bool IsEffectivelyMonitored(NzbDrone.Core.Series.Series series);

        void SetAuthorMonitored(int authorId, bool monitored);
        void SetSeriesMonitored(int seriesId, bool monitored);

        List<Book> GetEffectivelyMonitoredBooks();
        List<Audiobook> GetEffectivelyMonitoredAudiobooks();
    }
}
