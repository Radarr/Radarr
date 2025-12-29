using System.Collections.Generic;
using NzbDrone.Core.Audiobooks;
using NzbDrone.Core.Books;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Monitoring
{
    public interface IHierarchicalMonitoringService
    {
        bool IsEffectivelyMonitored(Book book);
        bool IsEffectivelyMonitored(Audiobook audiobook);
        bool IsEffectivelyMonitored(NzbDrone.Core.BookSeries.BookSeries bookSeries);
        bool IsEffectivelyMonitored(Album album);
        bool IsEffectivelyMonitored(Track track);

        void SetAuthorMonitored(int authorId, bool monitored);
        void SetBookSeriesMonitored(int bookSeriesId, bool monitored);
        void SetArtistMonitored(int artistId, bool monitored);
        void SetAlbumMonitored(int albumId, bool monitored);

        List<Book> GetEffectivelyMonitoredBooks();
        List<Audiobook> GetEffectivelyMonitoredAudiobooks();
        List<Track> GetEffectivelyMonitoredTracks();
    }
}
