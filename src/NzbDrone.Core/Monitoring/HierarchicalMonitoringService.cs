using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Audiobooks;
using NzbDrone.Core.Authors;
using NzbDrone.Core.Books;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Series;

namespace NzbDrone.Core.Monitoring
{
    // S107: Constructor has many parameters - this is consistent with DI patterns in this codebase
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S107", Justification = "DI pattern requires injecting services")]
    public class HierarchicalMonitoringService : IHierarchicalMonitoringService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly ISeriesRepository _seriesRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IAudiobookRepository _audiobookRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly ITrackRepository _trackRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public HierarchicalMonitoringService(
            IAuthorRepository authorRepository,
            ISeriesRepository seriesRepository,
            IBookRepository bookRepository,
            IAudiobookRepository audiobookRepository,
            IArtistRepository artistRepository,
            IAlbumRepository albumRepository,
            ITrackRepository trackRepository,
            IEventAggregator eventAggregator,
            Logger logger)
        {
            _authorRepository = authorRepository;
            _seriesRepository = seriesRepository;
            _bookRepository = bookRepository;
            _audiobookRepository = audiobookRepository;
            _artistRepository = artistRepository;
            _albumRepository = albumRepository;
            _trackRepository = trackRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public bool IsEffectivelyMonitored(Book book)
        {
            return book.Monitored && !IsAncestorUnmonitored(book.SeriesId, book.AuthorId);
        }

        public bool IsEffectivelyMonitored(Audiobook audiobook)
        {
            return audiobook.Monitored && !IsAncestorUnmonitored(audiobook.SeriesId, audiobook.AuthorId);
        }

        public bool IsEffectivelyMonitored(NzbDrone.Core.Series.Series series)
        {
            return series.Monitored && !IsAncestorUnmonitored(null, series.AuthorId);
        }

        public void SetAuthorMonitored(int authorId, bool monitored)
        {
            var author = _authorRepository.Get(authorId);
            if (author == null)
            {
                _logger.Warn("Author with id {0} not found", authorId);
                return;
            }

            var previousMonitored = author.Monitored;
            if (previousMonitored == monitored)
            {
                return;
            }

            author.Monitored = monitored;
            _authorRepository.Update(author);

            var changeEvent = new AuthorMonitoringChangedEvent(author, previousMonitored);

            // Cascade unmonitoring to descendants when author is unmonitored
            if (previousMonitored && !monitored)
            {
                CascadeUnmonitorFromAuthor(authorId, changeEvent);
            }

            _eventAggregator.PublishEvent(changeEvent);

            _logger.Info("Author {0} monitoring changed from {1} to {2}. Affected: {3} series, {4} books, {5} audiobooks",
                author.Name,
                previousMonitored,
                monitored,
                changeEvent.AffectedSeriesCount,
                changeEvent.AffectedBooksCount,
                changeEvent.AffectedAudiobooksCount);
        }

        public void SetSeriesMonitored(int seriesId, bool monitored)
        {
            var series = _seriesRepository.Get(seriesId);
            if (series == null)
            {
                _logger.Warn("Series with id {0} not found", seriesId);
                return;
            }

            var previousMonitored = series.Monitored;
            if (previousMonitored == monitored)
            {
                return;
            }

            series.Monitored = monitored;
            _seriesRepository.Update(series);

            var changeEvent = new SeriesMonitoringChangedEvent(series, previousMonitored);

            // Cascade unmonitoring to descendants when series is unmonitored
            if (previousMonitored && !monitored)
            {
                CascadeUnmonitorFromSeries(seriesId, changeEvent);
            }

            _eventAggregator.PublishEvent(changeEvent);

            _logger.Info("Series {0} monitoring changed from {1} to {2}. Affected: {3} books, {4} audiobooks",
                series.Title,
                previousMonitored,
                monitored,
                changeEvent.AffectedBooksCount,
                changeEvent.AffectedAudiobooksCount);
        }

        public List<Book> GetEffectivelyMonitoredBooks()
        {
            var (monitoredAuthors, monitoredSeries) = GetMonitoringContext();

            return _bookRepository.All()
                .Where(b => b.Monitored)
                .Where(b => !b.AuthorId.HasValue || monitoredAuthors.Contains(b.AuthorId.Value))
                .Where(b => !b.SeriesId.HasValue || monitoredSeries.Contains(b.SeriesId.Value))
                .ToList();
        }

        public List<Audiobook> GetEffectivelyMonitoredAudiobooks()
        {
            var (monitoredAuthors, monitoredSeries) = GetMonitoringContext();

            return _audiobookRepository.All()
                .Where(a => a.Monitored)
                .Where(a => !a.AuthorId.HasValue || monitoredAuthors.Contains(a.AuthorId.Value))
                .Where(a => !a.SeriesId.HasValue || monitoredSeries.Contains(a.SeriesId.Value))
                .ToList();
        }

        public bool IsEffectivelyMonitored(Album album)
        {
            return album.Monitored && !IsMusicAncestorUnmonitored(album.ArtistId);
        }

        public bool IsEffectivelyMonitored(Track track)
        {
            return track.Monitored && !IsMusicAncestorUnmonitored(track.AlbumId, null);
        }

        public void SetArtistMonitored(int artistId, bool monitored)
        {
            var artist = _artistRepository.Get(artistId);
            if (artist == null)
            {
                _logger.Warn("Artist with id {0} not found", artistId);
                return;
            }

            var previousMonitored = artist.Monitored;
            if (previousMonitored == monitored)
            {
                return;
            }

            artist.Monitored = monitored;
            _artistRepository.Update(artist);

            var changeEvent = new ArtistMonitoringChangedEvent(artist, previousMonitored);

            if (previousMonitored && !monitored)
            {
                CascadeUnmonitorFromArtist(artistId, changeEvent);
            }

            _eventAggregator.PublishEvent(changeEvent);

            _logger.Info("Artist {0} monitoring changed from {1} to {2}. Affected: {3} albums, {4} tracks",
                artist.Name,
                previousMonitored,
                monitored,
                changeEvent.AffectedAlbumsCount,
                changeEvent.AffectedTracksCount);
        }

        public void SetAlbumMonitored(int albumId, bool monitored)
        {
            var album = _albumRepository.Get(albumId);
            if (album == null)
            {
                _logger.Warn("Album with id {0} not found", albumId);
                return;
            }

            var previousMonitored = album.Monitored;
            if (previousMonitored == monitored)
            {
                return;
            }

            album.Monitored = monitored;
            _albumRepository.Update(album);

            var changeEvent = new AlbumMonitoringChangedEvent(album, previousMonitored);

            if (previousMonitored && !monitored)
            {
                CascadeUnmonitorFromAlbum(albumId, changeEvent);
            }

            _eventAggregator.PublishEvent(changeEvent);

            _logger.Info("Album {0} monitoring changed from {1} to {2}. Affected: {3} tracks",
                album.Title,
                previousMonitored,
                monitored,
                changeEvent.AffectedTracksCount);
        }

        public List<Track> GetEffectivelyMonitoredTracks()
        {
            var monitoredArtists = _artistRepository.GetMonitored()
                .Select(a => a.Id)
                .ToHashSet();

            var monitoredAlbums = _albumRepository.All()
                .Where(a => a.Monitored)
                .Where(a => !a.ArtistId.HasValue || monitoredArtists.Contains(a.ArtistId.Value))
                .Select(a => a.Id)
                .ToHashSet();

            return _trackRepository.All()
                .Where(t => t.Monitored)
                .Where(t => !t.AlbumId.HasValue || monitoredAlbums.Contains(t.AlbumId.Value))
                .ToList();
        }

        private bool IsMusicAncestorUnmonitored(int? artistId)
        {
            if (artistId.HasValue)
            {
                var artist = _artistRepository.Get(artistId.Value);
                if (artist != null && !artist.Monitored)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsMusicAncestorUnmonitored(int? albumId, int? artistId)
        {
            if (albumId.HasValue)
            {
                var album = _albumRepository.Get(albumId.Value);
                if (album != null)
                {
                    if (!album.Monitored)
                    {
                        return true;
                    }

                    if (album.ArtistId.HasValue)
                    {
                        return IsMusicAncestorUnmonitored(album.ArtistId);
                    }
                }
            }

            return IsMusicAncestorUnmonitored(artistId);
        }

        private void CascadeUnmonitorFromArtist(int artistId, ArtistMonitoringChangedEvent changeEvent)
        {
            var albumsToUnmonitor = _albumRepository.FindByArtistId(artistId).Where(a => a.Monitored).ToList();
            changeEvent.AffectedAlbumsCount = UnmonitorEntities(
                albumsToUnmonitor,
                a => a.Monitored = false,
                _albumRepository.UpdateMany);

            var albumIds = albumsToUnmonitor.Select(a => a.Id).ToList();
            changeEvent.AffectedTracksCount = UnmonitorEntities(
                albumIds.SelectMany(id => _trackRepository.FindByAlbumId(id)).Where(t => t.Monitored).ToList(),
                t => t.Monitored = false,
                _trackRepository.UpdateMany);
        }

        private void CascadeUnmonitorFromAlbum(int albumId, AlbumMonitoringChangedEvent changeEvent)
        {
            changeEvent.AffectedTracksCount = UnmonitorEntities(
                _trackRepository.FindByAlbumId(albumId).Where(t => t.Monitored).ToList(),
                t => t.Monitored = false,
                _trackRepository.UpdateMany);
        }

        private bool IsAncestorUnmonitored(int? seriesId, int? authorId)
        {
            if (seriesId.HasValue)
            {
                var series = _seriesRepository.Get(seriesId.Value);
                if (series != null && !series.Monitored)
                {
                    return true;
                }
            }

            if (authorId.HasValue)
            {
                var author = _authorRepository.Get(authorId.Value);
                if (author != null && !author.Monitored)
                {
                    return true;
                }
            }

            return false;
        }

        private (HashSet<int> MonitoredAuthors, HashSet<int> MonitoredSeries) GetMonitoringContext()
        {
            var monitoredAuthors = _authorRepository.GetMonitored()
                .Select(a => a.Id)
                .ToHashSet();

            var monitoredSeries = _seriesRepository.GetMonitored()
                .Where(s => !s.AuthorId.HasValue || monitoredAuthors.Contains(s.AuthorId.Value))
                .Select(s => s.Id)
                .ToHashSet();

            return (monitoredAuthors, monitoredSeries);
        }

        private void CascadeUnmonitorFromAuthor(int authorId, AuthorMonitoringChangedEvent changeEvent)
        {
            changeEvent.AffectedSeriesCount = UnmonitorEntities(
                _seriesRepository.FindByAuthorId(authorId).Where(s => s.Monitored).ToList(),
                s => s.Monitored = false,
                _seriesRepository.UpdateMany);

            changeEvent.AffectedBooksCount = UnmonitorEntities(
                _bookRepository.FindByAuthorId(authorId).Where(b => b.Monitored).ToList(),
                b => b.Monitored = false,
                _bookRepository.UpdateMany);

            changeEvent.AffectedAudiobooksCount = UnmonitorEntities(
                _audiobookRepository.FindByAuthorId(authorId).Where(a => a.Monitored).ToList(),
                a => a.Monitored = false,
                _audiobookRepository.UpdateMany);
        }

        private void CascadeUnmonitorFromSeries(int seriesId, SeriesMonitoringChangedEvent changeEvent)
        {
            changeEvent.AffectedBooksCount = UnmonitorEntities(
                _bookRepository.FindBySeriesId(seriesId).Where(b => b.Monitored).ToList(),
                b => b.Monitored = false,
                _bookRepository.UpdateMany);

            changeEvent.AffectedAudiobooksCount = UnmonitorEntities(
                _audiobookRepository.FindBySeriesId(seriesId).Where(a => a.Monitored).ToList(),
                a => a.Monitored = false,
                _audiobookRepository.UpdateMany);
        }

        private static int UnmonitorEntities<T>(List<T> entities, System.Action<T> setUnmonitored, System.Action<IList<T>> updateMany)
        {
            if (!entities.Any())
            {
                return 0;
            }

            foreach (var entity in entities)
            {
                setUnmonitored(entity);
            }

            updateMany(entities);
            return entities.Count;
        }
    }
}
