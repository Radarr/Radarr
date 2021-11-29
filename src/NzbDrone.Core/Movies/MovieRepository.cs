using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Movies
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        bool MoviePathExists(string path);
        List<Movie> FindByTitles(List<string> titles);
        Movie FindByImdbId(string imdbid);
        Movie FindByTmdbId(int tmdbid);
        List<Movie> FindByTmdbId(List<int> tmdbids);
        Movie FindByTitleSlug(string slug);
        List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec);
        List<Movie> GetMoviesByFileId(int fileId);
        void SetFileId(int fileId, int movieId);
        PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
        Movie FindByPath(string path);
        Dictionary<int, string> AllMoviePaths();
        Dictionary<int, string> AllMovieTitleSlugs();
        List<int> AllMovieTmdbIds();
        Dictionary<int, List<int>> AllMovieTags();
        List<int> GetRecommendations();
    }

    public class MovieRepository : BasicRepository<Movie>, IMovieRepository
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IAlternativeTitleRepository _alternativeTitleRepository;

        public MovieRepository(IMainDatabase database,
                               IProfileRepository profileRepository,
                               IAlternativeTitleRepository alternativeTitleRepository,
                               IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _profileRepository = profileRepository;
            _alternativeTitleRepository = alternativeTitleRepository;
        }

        protected override SqlBuilder Builder() => new SqlBuilder()
            .Join<Movie, Profile>((m, p) => m.ProfileId == p.Id)
            .LeftJoin<Movie, AlternativeTitle>((m, t) => m.Id == t.MovieId)
            .LeftJoin<Movie, MovieFile>((m, f) => m.Id == f.MovieId);

        private Movie Map(Dictionary<int, Movie> dict, Movie movie, Profile profile, AlternativeTitle altTitle, MovieFile movieFile, MovieTranslation translation = null)
        {
            Movie movieEntry;

            if (!dict.TryGetValue(movie.Id, out movieEntry))
            {
                movieEntry = movie;
                movieEntry.Profile = profile;
                movieEntry.MovieFile = movieFile;
                dict.Add(movieEntry.Id, movieEntry);
            }

            if (altTitle != null)
            {
                movieEntry.AlternativeTitles.Add(altTitle);
            }

            if (translation != null)
            {
                movieEntry.Translations.Add(translation);
            }

            return movieEntry;
        }

        protected override List<Movie> Query(SqlBuilder builder)
        {
            var movieDictionary = new Dictionary<int, Movie>();

            _ = _database.QueryJoined<Movie, Profile, AlternativeTitle, MovieFile>(
                builder,
                (movie, profile, altTitle, file) => Map(movieDictionary, movie, profile, altTitle, file));

            return movieDictionary.Values.ToList();
        }

        public override IEnumerable<Movie> All()
        {
            // the skips the join on profile and alternative title and populates manually
            // to avoid repeatedly deserializing the same profile / movie
            var builder = new SqlBuilder()
                .LeftJoin<Movie, MovieFile>((m, f) => m.MovieFileId == f.Id);

            var profiles = _profileRepository.All().ToDictionary(x => x.Id);
            var titles = _alternativeTitleRepository.All()
                .GroupBy(x => x.MovieId)
                .ToDictionary(x => x.Key, y => y.ToList());

            return _database.QueryJoined<Movie, MovieFile>(
                builder,
                (movie, file) =>
                {
                    movie.MovieFile = file;
                    movie.Profile = profiles[movie.ProfileId];

                    if (titles.TryGetValue(movie.Id, out var altTitles))
                    {
                        movie.AlternativeTitles = altTitles;
                    }

                    return movie;
                });
        }

        public bool MoviePathExists(string path)
        {
            return Query(x => x.Path == path).Any();
        }

        public List<Movie> FindByTitles(List<string> titles)
        {
            var distinct = titles.Distinct().ToList();

            var results = new List<Movie>();

            results.AddRange(FindByMovieTitles(distinct));
            results.AddRange(FindByAltTitles(distinct));
            results.AddRange(FindByTransTitles(distinct));

            return results.DistinctBy(x => x.Id).ToList();
        }

        // This is a bit of a hack, but if you try to combine / rationalise these then
        // SQLite makes a mess of the query plan and ends up doing a table scan
        private List<Movie> FindByMovieTitles(List<string> titles)
        {
            var movieDictionary = new Dictionary<int, Movie>();

            var builder = new SqlBuilder()
                .LeftJoin<Movie, AlternativeTitle>((m, t) => m.Id == t.MovieId)
                .LeftJoin<Movie, MovieFile>((m, f) => m.Id == f.MovieId)
                .LeftJoin<Movie, MovieTranslation>((m, tr) => m.Id == tr.MovieId)
                .Join<Movie, Profile>((m, p) => m.ProfileId == p.Id)
                .Where<Movie>(x => titles.Contains(x.CleanTitle));

            _ = _database.QueryJoined<Movie, Profile, AlternativeTitle, MovieFile, MovieTranslation>(
                builder,
                (movie, profile, altTitle, file, trans) => Map(movieDictionary, movie, profile, altTitle, file, trans));

            return movieDictionary.Values.ToList();
        }

        private List<Movie> FindByAltTitles(List<string> titles)
        {
            var movieDictionary = new Dictionary<int, Movie>();

            var builder = new SqlBuilder()
                .LeftJoin<AlternativeTitle, Movie>((t, m) => t.MovieId == m.Id)
                .LeftJoin<Movie, MovieFile>((m, f) => m.Id == f.MovieId)
                .LeftJoin<Movie, MovieTranslation>((m, tr) => m.Id == tr.MovieId)
                .Join<Movie, Profile>((m, p) => m.ProfileId == p.Id)
                .Where<AlternativeTitle>(x => titles.Contains(x.CleanTitle));

            _ = _database.QueryJoined<AlternativeTitle, Profile, Movie, MovieFile, MovieTranslation>(
                builder,
                (altTitle, profile, movie, file, trans) =>
                {
                    _ = Map(movieDictionary, movie, profile, altTitle, file, trans);
                    return null;
                });

            return movieDictionary.Values.ToList();
        }

        private List<Movie> FindByTransTitles(List<string> titles)
        {
            var movieDictionary = new Dictionary<int, Movie>();

            var builder = new SqlBuilder()
                .LeftJoin<MovieTranslation, Movie>((tr, m) => tr.MovieId == m.Id)
                .LeftJoin<Movie, AlternativeTitle>((m, t) => m.Id == t.MovieId)
                .LeftJoin<Movie, MovieFile>((m, f) => m.Id == f.MovieId)
                .Join<Movie, Profile>((m, p) => m.ProfileId == p.Id)
                .Where<MovieTranslation>(x => titles.Contains(x.CleanTitle));

            _ = _database.QueryJoined<MovieTranslation, Profile, Movie, MovieFile, AlternativeTitle>(
                builder,
                (trans, profile, movie, file, altTitle) =>
                {
                    _ = Map(movieDictionary, movie, profile, altTitle, file, trans);
                    return null;
                });

            return movieDictionary.Values.ToList();
        }

        public Movie FindByImdbId(string imdbid)
        {
            var imdbIdWithPrefix = Parser.Parser.NormalizeImdbId(imdbid);
            return imdbIdWithPrefix == null ? null : Query(x => x.ImdbId == imdbIdWithPrefix).FirstOrDefault();
        }

        public Movie FindByTmdbId(int tmdbid)
        {
            return Query(x => x.TmdbId == tmdbid).FirstOrDefault();
        }

        public List<Movie> FindByTmdbId(List<int> tmdbids)
        {
            return Query(x => tmdbids.Contains(x.TmdbId));
        }

        public List<Movie> GetMoviesByFileId(int fileId)
        {
            return Query(x => x.MovieFileId == fileId);
        }

        public void SetFileId(int fileId, int movieId)
        {
            SetFields(new Movie { Id = movieId, MovieFileId = fileId }, movie => movie.MovieFileId);
        }

        public Movie FindByTitleSlug(string slug)
        {
            return Query(x => x.TitleSlug == slug).FirstOrDefault();
        }

        public List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var builder = Builder()
                .Where<Movie>(m =>
                              (m.InCinemas >= start && m.InCinemas <= end) ||
                              (m.PhysicalRelease >= start && m.PhysicalRelease <= end) ||
                              (m.DigitalRelease >= start && m.DigitalRelease <= end));

            if (!includeUnmonitored)
            {
                builder.Where<Movie>(x => x.Monitored == true);
            }

            return Query(builder);
        }

        public SqlBuilder MoviesWithoutFilesBuilder() => Builder()
            .Where<Movie>(x => x.MovieFileId == 0);

        public PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec)
        {
            pagingSpec.Records = GetPagedRecords(MoviesWithoutFilesBuilder(), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(MoviesWithoutFilesBuilder().SelectCount(), pagingSpec);

            return pagingSpec;
        }

        public SqlBuilder MoviesWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff) => Builder()
                .Where<Movie>(x => x.MovieFileId != 0)
                .Where(BuildQualityCutoffWhereClause(qualitiesBelowCutoff));

        public PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            pagingSpec.Records = GetPagedRecords(MoviesWhereCutoffUnmetBuilder(qualitiesBelowCutoff), pagingSpec, PagedQuery);
            pagingSpec.TotalRecords = GetPagedRecordCount(MoviesWhereCutoffUnmetBuilder(qualitiesBelowCutoff).SelectCount(), pagingSpec);

            return pagingSpec;
        }

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format($"(\"{_table}\".\"ProfileId\" = {profile.ProfileId} AND \"MovieFiles\".\"Quality\" LIKE '%_quality_: {belowCutoff},%')"));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        public Movie FindByPath(string path)
        {
            return Query(x => x.Path == path).FirstOrDefault();
        }

        public Dictionary<int, string> AllMoviePaths()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT Id AS [Key], Path AS [Value] FROM Movies";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public Dictionary<int, string> AllMovieTitleSlugs()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT Id AS [Key], TitleSlug AS [Value] FROM Movies";
                return conn.Query<KeyValuePair<int, string>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public List<int> AllMovieTmdbIds()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<int>("SELECT TmdbId FROM Movies").ToList();
            }
        }

        public Dictionary<int, List<int>> AllMovieTags()
        {
            using (var conn = _database.OpenConnection())
            {
                var strSql = "SELECT Id AS [Key], Tags AS [Value] FROM Movies";
                return conn.Query<KeyValuePair<int, List<int>>>(strSql).ToDictionary(x => x.Key, x => x.Value);
            }
        }

        public List<int> GetRecommendations()
        {
            var recommendations = new List<int>();

            if (_database.Version < new Version("3.9.0"))
            {
                return recommendations;
            }

            using (var conn = _database.OpenConnection())
            {
                recommendations = conn.Query<int>(@"SELECT DISTINCT Rec FROM (
                                                    SELECT DISTINCT Rec FROM
                                                    (
                                                    SELECT DISTINCT CAST(j.value AS INT) AS Rec FROM Movies CROSS JOIN json_each(Movies.Recommendations) AS j
                                                    WHERE Rec NOT IN (SELECT TmdbId FROM Movies union SELECT TmdbId from ImportExclusions) LIMIT 10
                                                    )
                                                    UNION
                                                    SELECT Rec FROM
                                                    (
                                                    SELECT CAST(j.value AS INT) AS Rec FROM Movies CROSS JOIN json_each(Movies.Recommendations) AS j
                                                    WHERE Rec NOT IN (SELECT TmdbId FROM Movies union SELECT TmdbId from ImportExclusions)
                                                    GROUP BY Rec ORDER BY count(*) DESC LIMIT 120
                                                    )
                                                    )
                                                    LIMIT 100;").ToList();
            }

            return recommendations;
        }
    }
}
