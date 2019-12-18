using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.History
{
    public interface IHistoryRepository : IBasicRepository<History>
    {
        List<QualityModel> GetBestQualityInHistory(int movieId);
        History MostRecentForDownloadId(string downloadId);
        List<History> FindByDownloadId(string downloadId);
        List<History> FindDownloadHistory(int movieId, QualityModel quality);
        List<History> GetByMovieId(int movieId, HistoryEventType? eventType);
        void DeleteForMovie(int movieId);
        History MostRecentForMovie(int movieId);
        List<History> Since(DateTime date, HistoryEventType? eventType);
    }

    public class HistoryRepository : BasicRepository<History>, IHistoryRepository
    {
        public HistoryRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<QualityModel> GetBestQualityInHistory(int movieId)
        {
            var history = Query(x => x.MovieId == movieId);

            return history.Select(h => h.Quality).ToList();
        }

        public History MostRecentForDownloadId(string downloadId)
        {
            return FindByDownloadId(downloadId)
                .OrderByDescending(h => h.Date)
                .FirstOrDefault();
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return Query(x => x.DownloadId == downloadId);
        }

        public List<History> FindDownloadHistory(int movieId, QualityModel quality)
        {
            var allowed = new[] { HistoryEventType.Grabbed, HistoryEventType.DownloadFailed, HistoryEventType.DownloadFolderImported };

            return Query(h => h.MovieId == movieId &&
                         h.Quality == quality &&
                         allowed.Contains(h.EventType));
        }

        public List<History> GetByMovieId(int movieId, HistoryEventType? eventType)
        {
            var query = Query(x => x.MovieId == movieId);

            if (eventType.HasValue)
            {
                query = query.Where(h => h.EventType == eventType).ToList();
            }

            query.OrderByDescending(h => h.Date);

            return query;
        }

        public void DeleteForMovie(int movieId)
        {
            Delete(c => c.MovieId == movieId);
        }

        private IEnumerable<History> SelectJoined(SqlBuilder.Template sql)
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<History, Movie, Profile, History>(
                    sql.RawSql,
                    (hist, movie, profile) =>
                    {
                        hist.Movie = movie;
                        hist.Movie.Profile = profile;
                        return hist;
                    },
                    sql.Parameters)
                    .ToList();
            }
        }

        protected override SqlBuilder PagedBuilder() => new SqlBuilder()
            .Join<History, Movie>((h, m) => h.MovieId == m.Id)
            .Join<Movie, Profile>((m, p) => m.ProfileId == p.Id);

        protected override IEnumerable<History> PagedSelector(SqlBuilder.Template sql) => SelectJoined(sql);

        public History MostRecentForMovie(int movieId)
        {
            return Query(x => x.MovieId == movieId)
                .OrderByDescending(h => h.Date)
                .FirstOrDefault();
        }

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
