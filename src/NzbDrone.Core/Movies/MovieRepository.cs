using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Movies
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        bool MoviePathExists(string path);
        List<Movie> FindByTitles(List<string> titles);
        List<Movie> FindByTitleInexact(string cleanTitle);
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
        List<string> AllMoviePaths();
    }

    public class MovieRepository : BasicRepository<Movie>, IMovieRepository
    {
        private readonly IProfileRepository _profileRepository;
        public MovieRepository(IMainDatabase database,
                               IProfileRepository profileRepository,
                               IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _profileRepository = profileRepository;
        }

        protected override SqlBuilder BuilderBase() => new SqlBuilder()
            .Join<Movie, Profile>((m, p) => m.ProfileId == p.Id)
            .LeftJoin<Movie, AlternativeTitle>((m, t) => m.Id == t.MovieId)
            .LeftJoin<Movie, MovieFile>((m, f) => m.Id == f.MovieId);

        private Movie Map(Dictionary<int, Movie> dict, Movie movie, Profile profile, AlternativeTitle altTitle, MovieFile movieFile)
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

            return movieEntry;
        }

        protected override IEnumerable<Movie> GetResults(SqlBuilder.Template sql)
        {
            var movieDictionary = new Dictionary<int, Movie>();

            using (var conn = _database.OpenConnection())
            {
                conn.Query<Movie, Profile, AlternativeTitle, MovieFile, Movie>(
                    sql.RawSql,
                    (movie, profile, altTitle, file) => Map(movieDictionary, movie, profile, altTitle, file),
                    sql.Parameters);
            }

            return movieDictionary.Values;
        }

        public override IEnumerable<Movie> All()
        {
            // the skips the join on profile and populates manually
            // to avoid repeatedly deserializing the same profile
            var noProfileTemplate = $"SELECT /**select**/ FROM {_table} /**leftjoin**/ /**where**/ /**orderby**/";
            var sql = Builder().AddTemplate(noProfileTemplate).LogQuery();

            var movieDictionary = new Dictionary<int, Movie>();
            var profiles = _profileRepository.All().ToDictionary(x => x.Id);

            using (var conn = _database.OpenConnection())
            {
                conn.Query<Movie, AlternativeTitle, MovieFile, Movie>(
                    sql.RawSql,
                    (movie, altTitle, file) => Map(movieDictionary, movie, profiles[movie.ProfileId], altTitle, file),
                    sql.Parameters);
            }

            return movieDictionary.Values;
        }

        public bool MoviePathExists(string path)
        {
            return Query(x => x.Path == path).Any();
        }

        public List<Movie> FindByTitles(List<string> titles)
        {
            return Query(Builder().OrWhere<Movie>(x => titles.Contains(x.CleanTitle))
                         .OrWhere<AlternativeTitle>(x => titles.Contains(x.CleanTitle)));
        }

        public List<Movie> FindByTitleInexact(string cleanTitle)
        {
            return Query(x => cleanTitle.Contains(x.CleanTitle));
        }

        public Movie FindByImdbId(string imdbid)
        {
            var imdbIdWithPrefix = Parser.Parser.NormalizeImdbId(imdbid);
            return Query(x => x.ImdbId == imdbIdWithPrefix).FirstOrDefault();
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
                              (m.PhysicalRelease >= start && m.PhysicalRelease <= end));

            if (!includeUnmonitored)
            {
                builder.Where<Movie>(x => x.Monitored == true);
            }

            return Query(builder);
        }

        public SqlBuilder MoviesWithoutFilesBuilder() => BuilderBase().Where<Movie>(x => x.MovieFileId == 0);

        public PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec)
        {
            pagingSpec.Records = GetPagedRecords(MoviesWithoutFilesBuilder().SelectAll(), pagingSpec, PagedSelector);
            pagingSpec.TotalRecords = GetPagedRecordCount(MoviesWithoutFilesBuilder().SelectCount(), pagingSpec);

            return pagingSpec;
        }

        public SqlBuilder MoviesWhereCutoffUnmetBuilder(List<QualitiesBelowCutoff> qualitiesBelowCutoff) => BuilderBase()
                .Where<Movie>(x => x.MovieFileId != 0)
                .Where(BuildQualityCutoffWhereClause(qualitiesBelowCutoff));

        public PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            pagingSpec.Records = GetPagedRecords(MoviesWhereCutoffUnmetBuilder(qualitiesBelowCutoff).SelectAll(), pagingSpec, PagedSelector);
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

        public List<string> AllMoviePaths()
        {
            using (var conn = _database.OpenConnection())
            {
                return conn.Query<string>("SELECT Path FROM Movies").ToList();
            }
        }
    }
}
