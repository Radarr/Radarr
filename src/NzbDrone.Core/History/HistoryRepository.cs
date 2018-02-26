using System;
using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Tv;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<History>
    {
        List<QualityModel> GetBestQualityInHistory(int movieId);
        History MostRecentForDownloadId(string downloadId);
        List<History> FindByDownloadId(string downloadId);
        List<History> FindDownloadHistory(int idMovieId, QualityModel quality);
        void DeleteForMovie(int movieId);
        History MostRecentForMovie(int movieId);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {

        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<QualityModel> GetBestQualityInHistory(int movieId)
        {
            var history = Query.Where(c => c.MovieId == movieId);

            return history.Select(h => h.Quality).ToList();
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

        public List<History> FindDownloadHistory(int idMovieId, QualityModel quality)
        {
            return Query.Where(h =>
                 h.MovieId == idMovieId &&
                 h.Quality == quality &&
                 (h.EventType == HistoryEventType.Grabbed ||
                 h.EventType == HistoryEventType.DownloadFailed ||
                 h.EventType == HistoryEventType.DownloadFolderImported)
                 ).ToList();
        }

        public void DeleteForMovie(int movieId)
        {
            Delete(c => c.MovieId == movieId);
        }

        protected override SortBuilder<History> GetPagedQuery(QueryBuilder<History> query, PagingSpec<History> pagingSpec)
        {
            var baseQuery = query/*.Join<History, Series>(JoinType.Inner, h => h.Series, (h, s) => h.SeriesId == s.Id)
                                 .Join<History, Episode>(JoinType.Inner, h => h.Episode, (h, e) => h.EpisodeId == e.Id)*/
                                 .Join<History, Movie>(JoinType.Inner, h => h.Movie, (h, e) => h.MovieId == e.Id);



            return base.GetPagedQuery(baseQuery, pagingSpec);
        }

        public History MostRecentForMovie(int movieId)
        {
            return Query.Where(h => h.MovieId == movieId)
                        .OrderByDescending(h => h.Date)
                        .FirstOrDefault();
        }
    }
}
