using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;

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
            return Query.Where(h => h.AlbumId == albumId)
                        .OrderByDescending(h => h.Date)
                        .FirstOrDefault();
        }

        public History MostRecentForDownloadId(string downloadId)
        {
            return Query.Where(h => h.DownloadId == downloadId)
             .OrderByDescending(h => h.Date)
             .FirstOrDefault();
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return Query.Join<History, Artist>(JoinType.Left, h => h.Artist, (h, a) => h.ArtistId == a.Id)
                        .Join<History, Album>(JoinType.Left, h => h.Album, (h, r) => h.AlbumId == r.Id)
                        .Where(h => h.DownloadId == downloadId);
        }

        public List<History> GetByArtist(int artistId, HistoryEventType? eventType)
        {
            var query = Query.Where(h => h.ArtistId == artistId);

            if (eventType.HasValue)
            {
                query.AndWhere(h => h.EventType == eventType);
            }

            query.OrderByDescending(h => h.Date);

            return query;
        }

        public List<History> GetByAlbum(int albumId, HistoryEventType? eventType)
        {
            var query = Query.Join<History, Album>(JoinType.Inner, h => h.Album, (h, e) => h.AlbumId == e.Id)
                .Where(h => h.AlbumId == albumId);

            if (eventType.HasValue)
            {
                query.AndWhere(h => h.EventType == eventType);
            }

            query.OrderByDescending(h => h.Date);

            return query;
        }

        public List<History> FindDownloadHistory(int idArtistId, QualityModel quality)
        {
            return Query.Where(h =>
                 h.ArtistId == idArtistId &&
                 h.Quality == quality &&
                 (h.EventType == HistoryEventType.Grabbed ||
                 h.EventType == HistoryEventType.DownloadFailed ||
                 h.EventType == HistoryEventType.TrackFileImported)
                 ).ToList();
        }

        public void DeleteForArtist(int artistId)
        {
            Delete(c => c.ArtistId == artistId);
        }

        protected override SortBuilder<History> GetPagedQuery(QueryBuilder<History> query, PagingSpec<History> pagingSpec)
        {
            var baseQuery = query.Join<History, Artist>(JoinType.Inner, h => h.Artist, (h, a) => h.ArtistId == a.Id)
                                 .Join<History, Album>(JoinType.Inner, h => h.Album, (h, r) => h.AlbumId == r.Id)
                                 .Join<History, Track>(JoinType.Left, h => h.Track, (h, t) => h.TrackId == t.Id);

            return base.GetPagedQuery(baseQuery, pagingSpec);
        }

        public List<History> Since(DateTime date, HistoryEventType? eventType)
        {
            var query = Query.Where(h => h.Date >= date);

            if (eventType.HasValue)
            {
                query.AndWhere(h => h.EventType == eventType);
            }

            query.OrderBy(h => h.Date);

            return query;
        }
    }
}
