using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Datastore.Extensions;
using Marr.Data.QGen;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Parser.RomanNumerals;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Movies
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        bool MoviePathExists(string path);
        Movie FindByTitle(string cleanTitle);
        Movie FindByTitle(string cleanTitle, int year);
        List<Movie> FindByTitleInexact(string cleanTitle);
        Movie FindByImdbId(string imdbid);
        Movie FindByTmdbId(int tmdbid);
        Movie FindByTitleSlug(string slug);
        List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        List<Movie> MoviesWithFiles(int movieId);
        PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec);
        List<Movie> GetMoviesByFileId(int fileId);
        void SetFileId(int fileId, int movieId);
        PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
        Movie FindByPath(string path);
    }

    public class MovieRepository : BasicRepository<Movie>, IMovieRepository
    {
        protected IMainDatabase _database;

        public MovieRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
            _database = database;
        }

        public bool MoviePathExists(string path)
        {
            return Query.Where(c => c.Path == path).Any();
        }

        public Movie FindByTitle(string cleanTitle)
        {
            return FindByTitle(cleanTitle, null);
        }

        public Movie FindByTitle(string cleanTitle, int year)
        {
            return FindByTitle(cleanTitle, year as int?);
        }

        public Movie FindByImdbId(string imdbid)
        {
            var imdbIdWithPrefix = Parser.Parser.NormalizeImdbId(imdbid);
            return Query.Where(s => s.ImdbId == imdbIdWithPrefix).SingleOrDefault();
        }

        public List<Movie> GetMoviesByFileId(int fileId)
        {
            return Query.Where(m => m.MovieFileId == fileId).ToList();
        }

        public void SetFileId(int fileId, int movieId)
        {
            SetFields(new Movie { Id = movieId, MovieFileId = fileId }, movie => movie.MovieFileId);
        }

        public Movie FindByTitleSlug(string slug)
        {
            return Query.Where(m => m.TitleSlug == slug).FirstOrDefault();
        }

        public List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
                var query = Query.Where(m =>
                        (m.InCinemas >= start && m.InCinemas <= end) ||
                        (m.PhysicalRelease >= start && m.PhysicalRelease <= end));

                if (!includeUnmonitored)
                {
                    query.AndWhere(e => e.Monitored == true);
                }

                return query.ToList();
        }

        public List<Movie> MoviesWithFiles(int movieId)
        {
            return Query.Join<Movie, MovieFile>(JoinType.Inner, m => m.MovieFile, (m, mf) => m.MovieFileId == mf.Id)
                        .Where(m => m.Id == movieId).ToList();
        }

        public PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec)
        {
            pagingSpec.TotalRecords = GetMoviesWithoutFilesQuery(pagingSpec).GetRowCount();
            pagingSpec.Records = GetMoviesWithoutFilesQuery(pagingSpec).ToList();

            return pagingSpec;
        }

        public SortBuilder<Movie> GetMoviesWithoutFilesQuery(PagingSpec<Movie> pagingSpec)
        {
            return Query.Where(pagingSpec.FilterExpressions.FirstOrDefault())
                             .AndWhere(m => m.MovieFileId == 0)
                             .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                             .Skip(pagingSpec.PagingOffset())
                             .Take(pagingSpec.PageSize);
        }

        public PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            pagingSpec.TotalRecords = MoviesWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff).GetRowCount();
            pagingSpec.Records = MoviesWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff).ToList();

            return pagingSpec;
        }

        private SortBuilder<Movie> MoviesWhereCutoffUnmetQuery(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            return Query
                 .Join<Movie, MovieFile>(JoinType.Left, e => e.MovieFile, (e, s) => e.MovieFileId == s.Id)
                 .Where(pagingSpec.FilterExpressions.FirstOrDefault())
                 .AndWhere(m => m.MovieFileId != 0)
                 .AndWhere(BuildQualityCutoffWhereClause(qualitiesBelowCutoff))
                 .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                 .Skip(pagingSpec.PagingOffset())
                 .Take(pagingSpec.PageSize);
        }

        private string BuildQualityCutoffWhereClause(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("([t0].[ProfileId] = {0} AND [t2].[Quality] LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        private string BuildQualityCutoffWhereClauseSpecial(List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
            var clauses = new List<string>();

            foreach (var profile in qualitiesBelowCutoff)
            {
                foreach (var belowCutoff in profile.QualityIds)
                {
                    clauses.Add(string.Format("(Movies.ProfileId = {0} AND MovieFiles.Quality LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        private Movie FindByTitle(string cleanTitle, int? year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();
            string cleanTitleWithRomanNumbers = cleanTitle;
            string cleanTitleWithArabicNumbers = cleanTitle;

            foreach (ArabicRomanNumeral arabicRomanNumeral in RomanNumeralParser.GetArabicRomanNumeralsMapping())
            {
                string arabicNumber = arabicRomanNumeral.ArabicNumeralAsString;
                string romanNumber = arabicRomanNumeral.RomanNumeral;
                cleanTitleWithRomanNumbers = cleanTitleWithRomanNumbers.Replace(arabicNumber, romanNumber);
                cleanTitleWithArabicNumbers = cleanTitleWithArabicNumbers.Replace(romanNumber, arabicNumber);
            }

            Movie result = Query.Where(s => s.CleanTitle == cleanTitle).FirstWithYear(year);

            if (result == null)
            {
                result = Query.Where(movie => movie.CleanTitle == cleanTitleWithArabicNumbers || movie.CleanTitle == cleanTitleWithRomanNumbers)
                    .FirstWithYear(year);

                if (result == null)
                {
                    result = Query.Where<AlternativeTitle>(t => t.CleanTitle == cleanTitle || t.CleanTitle == cleanTitleWithArabicNumbers || t.CleanTitle == cleanTitleWithRomanNumbers)
                                  .FirstWithYear(year);
                }
            }

            return result;
        }

        public List<Movie> FindByTitleInexact(string cleanTitle)
        {
            var mapper = _database.GetDataMapper();
            mapper.AddParameter("queryTitle", cleanTitle);

            return AddJoinQueries(mapper.Query<Movie>()).Where($"instr(@queryTitle, [t0].[CleanTitle])");
        }

        public Movie FindByTmdbId(int tmdbid)
        {
            return Query.Where(m => m.TmdbId == tmdbid).FirstOrDefault();
        }

        public Movie FindByPath(string path)
        {
            return Query.Where(s => s.Path == path)
                        .FirstOrDefault();
        }

        protected override QueryBuilder<TActual> AddJoinQueries<TActual>(QueryBuilder<TActual> baseQuery)
        {
            baseQuery = base.AddJoinQueries(baseQuery);
            baseQuery = baseQuery.Join<Movie, AlternativeTitle>(JoinType.Left, m => m.AlternativeTitles,
                (m, t) => m.Id == t.MovieId);
            baseQuery = baseQuery.Join<Movie, MovieFile>(JoinType.Left, m => m.MovieFile, (m, f) => m.Id == f.MovieId);

            return baseQuery;
        }
    }
}
