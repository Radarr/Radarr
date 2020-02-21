using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<History>
    {
        History MostRecentForAlbum(int albumId);
        History MostRecentForDownloadId(string downloadId);
        List<History> FindByDownloadId(string downloadId);
        List<History> GetByArtist(int artistId, HistoryEventType? eventType);
        List<History> GetByAlbum(int albumId, HistoryEventType? eventType);
        List<History> FindDownloadHistory(int idArtistId, QualityModel quality);
        void DeleteForArtist(int artistId);
        List<History> Since(DateTime date, HistoryEventType? eventType);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {
        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public History MostRecentForAlbum(int albumId)
        {
            return Query(h => h.AlbumId == albumId)
                .OrderByDescending(h => h.Date)
                .FirstOrDefault();
        }

        public History MostRecentForDownloadId(string downloadId)
        {
            return Query(h => h.DownloadId == downloadId)
                .OrderByDescending(h => h.Date)
                .FirstOrDefault();
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return _database.QueryJoined<History, Artist, Album>(
                Builder()
                .Join<History, Artist>((h, a) => h.ArtistId == a.Id)
                .Join<History, Album>((h, a) => h.AlbumId == a.Id)
                .Where<History>(h => h.DownloadId == downloadId),
                (history, artist, album) =>
                {
                    history.Artist = artist;
                    history.Album = album;
                    return history;
                }).ToList();
        }

        public List<History> GetByArtist(int artistId, HistoryEventType? eventType)
        {
            var builder = Builder().Where<History>(h => h.ArtistId == artistId);

            if (eventType.HasValue)
            {
                builder.Where<History>(h => h.EventType == eventType);
            }

            return Query(builder).OrderByDescending(h => h.Date).ToList();
        }

        public List<History> GetByAlbum(int albumId, HistoryEventType? eventType)
        {
            var builder = Builder()
                .Join<History, Album>((h, a) => h.AlbumId == a.Id)
                .Where<History>(h => h.AlbumId == albumId);

            if (eventType.HasValue)
            {
                builder.Where<History>(h => h.EventType == eventType);
            }

            return _database.QueryJoined<History, Album>(
                builder,
                (history, album) =>
                {
                    history.Album = album;
                    return history;
                }).OrderByDescending(h => h.Date).ToList();
        }

        public List<History> FindDownloadHistory(int idArtistId, QualityModel quality)
        {
            var allowed = new[] { HistoryEventType.Grabbed, HistoryEventType.DownloadFailed, HistoryEventType.TrackFileImported };

            return Query(h => h.ArtistId == idArtistId &&
                         h.Quality == quality &&
                         allowed.Contains(h.EventType));
        }

        public void DeleteForArtist(int artistId)
        {
            Delete(c => c.ArtistId == artistId);
        }

        protected override SqlBuilder PagedBuilder() => new SqlBuilder()
            .Join<History, Artist>((h, a) => h.ArtistId == a.Id)
            .Join<History, Album>((h, a) => h.AlbumId == a.Id)
            .LeftJoin<History, Track>((h, t) => h.TrackId == t.Id);
        protected override IEnumerable<History> PagedQuery(SqlBuilder builder) =>
            _database.QueryJoined<History, Artist, Album, Track>(builder, (history, artist, album, track) =>
                    {
                        history.Artist = artist;
                        history.Album = album;
                        history.Track = track;
                        return history;
                    });

        public List<History> Since(DateTime date, HistoryEventType? eventType)
        {
            var builder = Builder().Where<History>(x => x.Date >= date);

            if (eventType.HasValue)
            {
                builder.Where<History>(h => h.EventType == eventType);
            }

            return Query(builder).OrderBy(h => h.Date).ToList();
        }
    }
}
