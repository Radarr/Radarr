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
    public interface IHistoryRepository : IBasicRepository<MovieHistory>
    {
        List<QualityModel> GetBestQualityInHistory(int movieId);
        MovieHistory MostRecentForDownloadId(string downloadId);
        List<MovieHistory> FindByDownloadId(string downloadId);
        List<MovieHistory> FindDownloadHistory(int movieId, QualityModel quality);
        List<MovieHistory> GetByMovieId(int movieId, MovieHistoryEventType? eventType);
        void DeleteForMovie(int movieId);
        MovieHistory MostRecentForMovie(int movieId);
        List<MovieHistory> Since(DateTime date, MovieHistoryEventType? eventType);
    }

    public class HistoryRepository : BasicRepository<MovieHistory>, IHistoryRepository
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

        public MovieHistory MostRecentForDownloadId(string downloadId)
        {
            return FindByDownloadId(downloadId)
                .OrderByDescending(h => h.Date)
                .FirstOrDefault();
        }

        public List<MovieHistory> FindByDownloadId(string downloadId)
        {
            return Query(x => x.DownloadId == downloadId);
        }

        public List<MovieHistory> FindDownloadHistory(int movieId, QualityModel quality)
        {
            var allowed = new[] { MovieHistoryEventType.Grabbed, MovieHistoryEventType.DownloadFailed, MovieHistoryEventType.DownloadFolderImported };

            return Query(h => h.MovieId == movieId &&
                         h.Quality == quality &&
                         allowed.Contains(h.EventType));
        }

        public List<MovieHistory> GetByMovieId(int movieId, MovieHistoryEventType? eventType)
        {
            var query = Query(x => x.MovieId == movieId);

            if (eventType.HasValue)
            {
                query = query.Where(h => h.EventType == eventType).ToList();
            }

            return query.OrderByDescending(h => h.Date).ToList();
        }

        public void DeleteForMovie(int movieId)
        {
            Delete(c => c.MovieId == movieId);
        }

        private IEnumerable<MovieHistory> SelectJoined(SqlBuilder.Template sql)
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<MovieHistory, Movie, Profile, MovieHistory>(
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
            .Join<MovieHistory, Movie>((h, m) => h.MovieId == m.Id)
            .Join<Movie, Profile>((m, p) => m.ProfileId == p.Id);

        protected override IEnumerable<MovieHistory> PagedSelector(SqlBuilder.Template sql) => SelectJoined(sql);

        public MovieHistory MostRecentForMovie(int movieId)
        {
            return Query(x => x.MovieId == movieId)
                .OrderByDescending(h => h.Date)
                .FirstOrDefault();
        }

        public List<MovieHistory> Since(DateTime date, MovieHistoryEventType? eventType)
        {
            var builder = Builder().Where<MovieHistory>(x => x.Date >= date);

            if (eventType.HasValue)
            {
                builder.Where<MovieHistory>(h => h.EventType == eventType);
            }

            return Query(builder).OrderBy(h => h.Date).ToList();
        }
    }
}
