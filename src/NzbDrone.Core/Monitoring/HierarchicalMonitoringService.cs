using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Core.Audiobooks;
using NzbDrone.Core.Authors;
using NzbDrone.Core.Books;
using NzbDrone.Core.BookSeries;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Monitoring.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.TV;

namespace NzbDrone.Core.Monitoring
{
    // S107: Constructor has many parameters - this is consistent with DI patterns in this codebase
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S107", Justification = "DI pattern requires injecting services")]
    public class HierarchicalMonitoringService : IHierarchicalMonitoringService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IBookSeriesRepository _bookSeriesRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IAudiobookRepository _audiobookRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IAlbumRepository _albumRepository;
        private readonly ITrackRepository _trackRepository;
        private readonly ITVShowRepository _tvShowRepository;
        private readonly ISeasonRepository _seasonRepository;
        private readonly IEpisodeRepository _episodeRepository;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public HierarchicalMonitoringService(
            IAuthorRepository authorRepository,
            IBookSeriesRepository bookSeriesRepository,
            IBookRepository bookRepository,
            IAudiobookRepository audiobookRepository,
            IArtistRepository artistRepository,
            IAlbumRepository albumRepository,
            ITrackRepository trackRepository,
            ITVShowRepository tvShowRepository,
            ISeasonRepository seasonRepository,
            IEpisodeRepository episodeRepository,
            IEventAggregator eventAggregator,
            Logger logger)
        {
            _authorRepository = authorRepository;
            _bookSeriesRepository = bookSeriesRepository;
            _bookRepository = bookRepository;
            _audiobookRepository = audiobookRepository;
            _artistRepository = artistRepository;
            _albumRepository = albumRepository;
            _trackRepository = trackRepository;
            _tvShowRepository = tvShowRepository;
            _seasonRepository = seasonRepository;
            _episodeRepository = episodeRepository;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public bool IsEffectivelyMonitored(Book book)
        {
            return book.Monitored && !IsAncestorUnmonitored(book.BookSeriesId, book.AuthorId);
        }

        public bool IsEffectivelyMonitored(Audiobook audiobook)
        {
            return audiobook.Monitored && !IsAncestorUnmonitored(audiobook.BookSeriesId, audiobook.AuthorId);
        }

        public bool IsEffectivelyMonitored(NzbDrone.Core.BookSeries.BookSeries bookSeries)
        {
            return bookSeries.Monitored && !IsAncestorUnmonitored(null, bookSeries.AuthorId);
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

            _logger.Info("Author {0} monitoring changed from {1} to {2}. Affected: {3} book series, {4} books, {5} audiobooks",
                author.Name,
                previousMonitored,
                monitored,
                changeEvent.AffectedSeriesCount,
                changeEvent.AffectedBooksCount,
                changeEvent.AffectedAudiobooksCount);
        }

        public void SetBookSeriesMonitored(int bookSeriesId, bool monitored)
        {
            var bookSeries = _bookSeriesRepository.Get(bookSeriesId);
            if (bookSeries == null)
            {
                _logger.Warn("BookSeries with id {0} not found", bookSeriesId);
                return;
            }

            var previousMonitored = bookSeries.Monitored;
            if (previousMonitored == monitored)
            {
                return;
            }

            bookSeries.Monitored = monitored;
            _bookSeriesRepository.Update(bookSeries);

            var changeEvent = new BookSeriesMonitoringChangedEvent(bookSeries, previousMonitored);

            // Cascade unmonitoring to descendants when book series is unmonitored
            if (previousMonitored && !monitored)
            {
                CascadeUnmonitorFromBookSeries(bookSeriesId, changeEvent);
            }

            _eventAggregator.PublishEvent(changeEvent);

            _logger.Info("BookSeries {0} monitoring changed from {1} to {2}. Affected: {3} books, {4} audiobooks",
                bookSeries.Title,
                previousMonitored,
                monitored,
                changeEvent.AffectedBooksCount,
                changeEvent.AffectedAudiobooksCount);
        }

        public List<Book> GetEffectivelyMonitoredBooks()
        {
            var (monitoredAuthors, monitoredBookSeries) = GetMonitoringContext();

            return _bookRepository.All()
                .Where(b => b.Monitored)
                .Where(b => !b.AuthorId.HasValue || monitoredAuthors.Contains(b.AuthorId.Value))
                .Where(b => !b.BookSeriesId.HasValue || monitoredBookSeries.Contains(b.BookSeriesId.Value))
                .ToList();
        }

        public List<Audiobook> GetEffectivelyMonitoredAudiobooks()
        {
            var (monitoredAuthors, monitoredBookSeries) = GetMonitoringContext();

            return _audiobookRepository.All()
                .Where(a => a.Monitored)
                .Where(a => !a.AuthorId.HasValue || monitoredAuthors.Contains(a.AuthorId.Value))
                .Where(a => !a.BookSeriesId.HasValue || monitoredBookSeries.Contains(a.BookSeriesId.Value))
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

        public bool IsEffectivelyMonitored(Episode episode)
        {
            return episode.Monitored && !IsTVAncestorUnmonitored(episode.SeasonId, episode.TVShowId);
        }

        public bool IsEffectivelyMonitored(Season season)
        {
            return season.Monitored && !IsTVAncestorUnmonitored(null, season.TVShowId);
        }

        public void SetTVShowMonitored(int tvShowId, bool monitored)
        {
            var tvShow = _tvShowRepository.Get(tvShowId);
            if (tvShow == null)
            {
                _logger.Warn("TVShow with id {0} not found", tvShowId);
                return;
            }

            var previousMonitored = tvShow.Monitored;
            if (previousMonitored == monitored)
            {
                return;
            }

            tvShow.Monitored = monitored;
            _tvShowRepository.Update(tvShow);

            var changeEvent = new TVShowMonitoringChangedEvent(tvShow, previousMonitored);

            if (previousMonitored && !monitored)
            {
                CascadeUnmonitorFromTVShow(tvShowId, changeEvent);
            }

            _eventAggregator.PublishEvent(changeEvent);

            _logger.Info("TVShow {0} monitoring changed from {1} to {2}. Affected: {3} seasons, {4} episodes",
                tvShow.Title,
                previousMonitored,
                monitored,
                changeEvent.AffectedSeasonsCount,
                changeEvent.AffectedEpisodesCount);
        }

        public void SetSeasonMonitored(int seasonId, bool monitored)
        {
            var season = _seasonRepository.Get(seasonId);
            if (season == null)
            {
                _logger.Warn("Season with id {0} not found", seasonId);
                return;
            }

            var previousMonitored = season.Monitored;
            if (previousMonitored == monitored)
            {
                return;
            }

            season.Monitored = monitored;
            _seasonRepository.Update(season);

            var changeEvent = new SeasonMonitoringChangedEvent(season, previousMonitored);

            if (previousMonitored && !monitored)
            {
                CascadeUnmonitorFromSeason(seasonId, changeEvent);
            }

            _eventAggregator.PublishEvent(changeEvent);

            _logger.Info("Season {0} monitoring changed from {1} to {2}. Affected: {3} episodes",
                season.SeasonNumber,
                previousMonitored,
                monitored,
                changeEvent.AffectedEpisodesCount);
        }

        public List<Episode> GetEffectivelyMonitoredEpisodes()
        {
            var monitoredTVShows = _tvShowRepository.GetMonitored()
                .Select(t => t.Id)
                .ToHashSet();

            var monitoredSeasons = _seasonRepository.All()
                .Where(s => s.Monitored)
                .Where(s => !s.TVShowId.HasValue || monitoredTVShows.Contains(s.TVShowId.Value))
                .Select(s => s.Id)
                .ToHashSet();

            return _episodeRepository.All()
                .Where(e => e.Monitored)
                .Where(e => !e.SeasonId.HasValue || monitoredSeasons.Contains(e.SeasonId.Value))
                .ToList();
        }

        private bool IsTVAncestorUnmonitored(int? seasonId, int? tvShowId)
        {
            if (seasonId.HasValue)
            {
                var season = _seasonRepository.Get(seasonId.Value);
                if (season != null && !season.Monitored)
                {
                    return true;
                }
            }

            if (tvShowId.HasValue)
            {
                var tvShow = _tvShowRepository.Get(tvShowId.Value);
                if (tvShow != null && !tvShow.Monitored)
                {
                    return true;
                }
            }

            return false;
        }

        private void CascadeUnmonitorFromTVShow(int tvShowId, TVShowMonitoringChangedEvent changeEvent)
        {
            var seasonsToUnmonitor = _seasonRepository.FindByTVShowId(tvShowId).Where(s => s.Monitored).ToList();
            changeEvent.AffectedSeasonsCount = UnmonitorEntities(
                seasonsToUnmonitor,
                s => s.Monitored = false,
                _seasonRepository.UpdateMany);

            var seasonIds = seasonsToUnmonitor.Select(s => s.Id).ToList();
            changeEvent.AffectedEpisodesCount = UnmonitorEntities(
                seasonIds.SelectMany(id => _episodeRepository.FindBySeasonId(id)).Where(e => e.Monitored).ToList(),
                e => e.Monitored = false,
                _episodeRepository.UpdateMany);
        }

        private void CascadeUnmonitorFromSeason(int seasonId, SeasonMonitoringChangedEvent changeEvent)
        {
            changeEvent.AffectedEpisodesCount = UnmonitorEntities(
                _episodeRepository.FindBySeasonId(seasonId).Where(e => e.Monitored).ToList(),
                e => e.Monitored = false,
                _episodeRepository.UpdateMany);
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

        private bool IsAncestorUnmonitored(int? bookSeriesId, int? authorId)
        {
            if (bookSeriesId.HasValue)
            {
                var bookSeries = _bookSeriesRepository.Get(bookSeriesId.Value);
                if (bookSeries != null && !bookSeries.Monitored)
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

        private (HashSet<int> MonitoredAuthors, HashSet<int> MonitoredBookSeries) GetMonitoringContext()
        {
            var monitoredAuthors = _authorRepository.GetMonitored()
                .Select(a => a.Id)
                .ToHashSet();

            var monitoredBookSeries = _bookSeriesRepository.GetMonitored()
                .Where(s => !s.AuthorId.HasValue || monitoredAuthors.Contains(s.AuthorId.Value))
                .Select(s => s.Id)
                .ToHashSet();

            return (monitoredAuthors, monitoredBookSeries);
        }

        private void CascadeUnmonitorFromAuthor(int authorId, AuthorMonitoringChangedEvent changeEvent)
        {
            changeEvent.AffectedSeriesCount = UnmonitorEntities(
                _bookSeriesRepository.FindByAuthorId(authorId).Where(s => s.Monitored).ToList(),
                s => s.Monitored = false,
                _bookSeriesRepository.UpdateMany);

            changeEvent.AffectedBooksCount = UnmonitorEntities(
                _bookRepository.FindByAuthorId(authorId).Where(b => b.Monitored).ToList(),
                b => b.Monitored = false,
                _bookRepository.UpdateMany);

            changeEvent.AffectedAudiobooksCount = UnmonitorEntities(
                _audiobookRepository.FindByAuthorId(authorId).Where(a => a.Monitored).ToList(),
                a => a.Monitored = false,
                _audiobookRepository.UpdateMany);
        }

        private void CascadeUnmonitorFromBookSeries(int bookSeriesId, BookSeriesMonitoringChangedEvent changeEvent)
        {
            changeEvent.AffectedBooksCount = UnmonitorEntities(
                _bookRepository.FindByBookSeriesId(bookSeriesId).Where(b => b.Monitored).ToList(),
                b => b.Monitored = false,
                _bookRepository.UpdateMany);

            changeEvent.AffectedAudiobooksCount = UnmonitorEntities(
                _audiobookRepository.FindByBookSeriesId(bookSeriesId).Where(a => a.Monitored).ToList(),
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
