using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Profiles.Qualities;
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
        void DeleteForMovies(List<int> movieIds);
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
            return FindByDownloadId(downloadId).MaxBy(h => h.Date);
        }

        public List<MovieHistory> FindByDownloadId(string downloadId)
        {
            return Query(x => x.DownloadId == downloadId);
        }

        public List<MovieHistory> FindDownloadHistory(int movieId, QualityModel quality)
        {
            var allowed = new[] { (int)MovieHistoryEventType.Grabbed, (int)MovieHistoryEventType.DownloadFailed, (int)MovieHistoryEventType.DownloadFolderImported };

            return Query(h => h.MovieId == movieId &&
                         h.Quality == quality &&
                         allowed.Contains((int)h.EventType));
        }

        public List<MovieHistory> GetByMovieId(int movieId, MovieHistoryEventType? eventType)
        {
            var builder = new SqlBuilder(_database.DatabaseType)
                .Join<MovieHistory, Movie>((h, m) => h.MovieId == m.Id)
                .Join<Movie, QualityProfile>((m, p) => m.QualityProfileId == p.Id)
                .Where<MovieHistory>(h => h.MovieId == movieId);

            if (eventType.HasValue)
            {
                builder.Where<MovieHistory>(h => h.EventType == eventType);
            }

            return PagedQuery(builder).OrderByDescending(h => h.Date).ToList();
        }

        public void DeleteForMovies(List<int> movieIds)
        {
            Delete(c => movieIds.Contains(c.MovieId));
        }

        protected override SqlBuilder PagedBuilder() => new SqlBuilder(_database.DatabaseType)
            .Join<MovieHistory, Movie>((h, m) => h.MovieId == m.Id)
            .Join<Movie, QualityProfile>((m, p) => m.QualityProfileId == p.Id)
            .LeftJoin<Movie, MovieMetadata>((m, mm) => m.MovieMetadataId == mm.Id);

        protected override IEnumerable<MovieHistory> PagedQuery(SqlBuilder sql) =>
            _database.QueryJoined<MovieHistory, Movie, QualityProfile>(sql, (hist, movie, profile) =>
                    {
                        hist.Movie = movie;
                        hist.Movie.QualityProfile = profile;
                        return hist;
                    });

        public MovieHistory MostRecentForMovie(int movieId)
        {
            return Query(x => x.MovieId == movieId).MaxBy(h => h.Date);
        }

        public List<MovieHistory> Since(DateTime date, MovieHistoryEventType? eventType)
        {
            var builder = new SqlBuilder(_database.DatabaseType)
                .Join<MovieHistory, Movie>((h, m) => h.MovieId == m.Id)
                .Join<Movie, QualityProfile>((m, p) => m.QualityProfileId == p.Id)
                .Where<MovieHistory>(x => x.Date >= date);

            if (eventType.HasValue)
            {
                builder.Where<MovieHistory>(h => h.EventType == eventType);
            }

            return PagedQuery(builder).OrderBy(h => h.Date).ToList();
        }
    }
}
