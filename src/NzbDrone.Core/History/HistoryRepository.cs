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
        List<QualityModel> GetBestQualityInHistory(int albumId);
        History MostRecentForAlbum(int albumId);
        History MostRecentForDownloadId(string downloadId);
        List<History> FindByDownloadId(string downloadId);
        List<History> FindDownloadHistory(int idArtistId, QualityModel quality);
        void DeleteForArtist(int artistId);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {

        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }


        public List<QualityModel> GetBestQualityInHistory(int albumId)
        {
            var history = Query.Where(c => c.AlbumId == albumId);

            return history.Select(h => h.Quality).ToList();
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
            return Query.Where(h => h.DownloadId == downloadId);
        }

        public List<History> FindDownloadHistory(int idArtistId, QualityModel quality)
        {
            return Query.Where(h =>
                 h.ArtistId == idArtistId &&
                 h.Quality == quality &&
                 (h.EventType == HistoryEventType.Grabbed ||
                 h.EventType == HistoryEventType.DownloadFailed ||
                 h.EventType == HistoryEventType.DownloadFolderImported)
                 ).ToList();
        }

        public void DeleteForArtist(int artistId)
        {
            Delete(c => c.ArtistId == artistId);
        }

        protected override SortBuilder<History> GetPagedQuery(QueryBuilder<History> query, PagingSpec<History> pagingSpec)
        {
            var baseQuery = query.Join<History, Artist>(JoinType.Inner, h => h.Artist, (h, s) => h.ArtistId == s.Id)
                                 .Join<History, Album>(JoinType.Inner, h => h.Album, (h, e) => h.AlbumId == e.Id);

            return base.GetPagedQuery(baseQuery, pagingSpec);
        }
    }
}