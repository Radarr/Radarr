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
using CoreParser = NzbDrone.Core.Parser.Parser;

namespace NzbDrone.Core.Tv
{
    public interface IMovieRepository : IBasicRepository<Movie>
    {
        bool MoviePathExists(string path);
        Movie FindByTitle(string cleanTitle);
        Movie FindByTitle(string cleanTitle, int year);
        Movie FindByImdbId(string imdbid);
        Movie FindByTmdbId(int tmdbid);
        Movie FindByTitleSlug(string slug);
        List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored);
        List<Movie> MoviesWithFiles(int movieId);
        PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec);
        List<Movie> GetMoviesByFileId(int fileId);
        void SetFileId(int fileId, int movieId);
        PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff);
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

        public void SetFileId(int fileId, int episodeId)
        {
            SetFields(new Movie { Id = episodeId, MovieFileId = fileId }, movie => movie.MovieFileId);
        }

        public Movie FindByTitleSlug(string slug)
        {
            return Query.FirstOrDefault(m => m.TitleSlug == slug);
        }

        public List<Movie> MoviesBetweenDates(DateTime start, DateTime end, bool includeUnmonitored)
        {
            var query = Query.Where(m => m.InCinemas >= start && m.InCinemas <= end).OrWhere(m => m.PhysicalRelease >= start && m.PhysicalRelease <= end);

            if (!includeUnmonitored)
            {
                query.AndWhere(e => e.Monitored);
            }

            return query.ToList();
        }

        public List<Movie> MoviesWithFiles(int movieId)
        {
            return Query.Join<Movie, MovieFile>(JoinType.Inner, m => m.MovieFile, (m, mf) => m.MovieFileId == mf.Id)
                        .Where(m => m.Id == movieId);
        }

        public PagingSpec<Movie> MoviesWithoutFiles(PagingSpec<Movie> pagingSpec)
        {

            pagingSpec.TotalRecords = GetMoviesWithoutFilesQuery(pagingSpec).GetRowCount();
            pagingSpec.Records = GetMoviesWithoutFilesQuery(pagingSpec).ToList();

            return pagingSpec;
        }

        public override PagingSpec<Movie> GetPaged(PagingSpec<Movie> pagingSpec)
		{
			if (pagingSpec.SortKey == "downloadedQuality")
			{
				var mapper = _database.GetDataMapper();
				var offset = pagingSpec.PagingOffset();
				var limit = pagingSpec.PageSize;
				var direction = "ASC";
				if (pagingSpec.SortDirection == NzbDrone.Core.Datastore.SortDirection.Descending)
				{
					direction = "DESC";
				}
				var q = mapper.Query<Movie>($"SELECT * from \"Movies\" , \"MovieFiles\", \"QualityDefinitions\" WHERE Movies.MovieFileId=MovieFiles.Id AND instr(MovieFiles.Quality, ('quality\": ' || QualityDefinitions.Quality || \",\")) > 0 ORDER BY QualityDefinitions.Title {direction} LIMIT {offset},{limit};");
				var q2 = mapper.Query<Movie>("SELECT * from \"Movies\" , \"MovieFiles\", \"QualityDefinitions\" WHERE Movies.MovieFileId=MovieFiles.Id AND instr(MovieFiles.Quality, ('quality\": ' || QualityDefinitions.Quality || \",\")) > 0 ORDER BY QualityDefinitions.Title ASC;");

				//var ok = q.BuildQuery();

				pagingSpec.Records = q.ToList();
				pagingSpec.TotalRecords = q2.Count();

			}
			else
			{
				pagingSpec = base.GetPaged(pagingSpec);
			    //pagingSpec.Records = GetPagedQuery(Query, pagingSpec).ToList();
			    //pagingSpec.TotalRecords = GetPagedQuery(Query, pagingSpec).GetRowCount();
			}

			if (pagingSpec.Records.Count == 0 && pagingSpec.Page != 1)
			{
				var lastPossiblePage = pagingSpec.TotalRecords / pagingSpec.PageSize + 1;
				pagingSpec.Page = lastPossiblePage;
				return GetPaged(pagingSpec);
			}

			return pagingSpec;
		}
        
        /*protected override SortBuilder<Movie> GetPagedQuery(QueryBuilder<Movie> query, PagingSpec<Movie> pagingSpec)
        {
            return DataMapper.Query<Movie>().Join<Movie, AlternativeTitle>(JoinType.Left, m => m.AlternativeTitles,
                (m, t) => m.Id == t.MovieId).Where(pagingSpec.FilterExpression)
                .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                .Skip(pagingSpec.PagingOffset())
                .Take(pagingSpec.PageSize);
        }*/
        
        /*protected override SortBuilder<Movie> GetPagedQuery(QueryBuilder<Movie> query, PagingSpec<Movie> pagingSpec)
        {
            var newQuery = base.GetPagedQuery(query.Join<Movie, AlternativeTitle>(JoinType.Left, m => m.JoinAlternativeTitles, (movie, title) => title.MovieId == movie.Id), pagingSpec);
            System.Console.WriteLine(newQuery.ToString());
            return newQuery;
        }*/

        public SortBuilder<Movie> GetMoviesWithoutFilesQuery(PagingSpec<Movie> pagingSpec)
        {
            return Query.Where(pagingSpec.FilterExpression)
                             .AndWhere(m => m.MovieFileId == 0)
                             .OrderBy(pagingSpec.OrderByClause(), pagingSpec.ToSortDirection())
                             .Skip(pagingSpec.PagingOffset())
                             .Take(pagingSpec.PageSize);
        }

        public PagingSpec<Movie> MoviesWhereCutoffUnmet(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
        {
			//I know this is bad, but if you have a better Idea please tell me.
			if (pagingSpec.SortKey == "downloadedQuality")
			{
				var mapper = _database.GetDataMapper();
				var offset = pagingSpec.PagingOffset();
				var limit = pagingSpec.PageSize;
				var direction = "ASC";
				if (pagingSpec.SortDirection == NzbDrone.Core.Datastore.SortDirection.Descending)
				{
					direction = "DESC";
				}

				var whereClause = BuildQualityCutoffWhereClauseSpecial(qualitiesBelowCutoff);

				var q = mapper.Query<Movie>($"SELECT * from \"Movies\" , \"MovieFiles\", \"QualityDefinitions\" WHERE Movies.MovieFileId=MovieFiles.Id AND instr(MovieFiles.Quality, ('quality\": ' || QualityDefinitions.Quality || \",\")) > 0 AND {whereClause} ORDER BY QualityDefinitions.Title {direction} LIMIT {offset},{limit};");
				var q2 = mapper.Query<Movie>($"SELECT * from \"Movies\" , \"MovieFiles\", \"QualityDefinitions\" WHERE Movies.MovieFileId=MovieFiles.Id AND instr(MovieFiles.Quality, ('quality\": ' || QualityDefinitions.Quality || \",\")) > 0 AND {whereClause} ORDER BY QualityDefinitions.Title ASC;");

				//var ok = q.BuildQuery();

				pagingSpec.Records = q.ToList();
				pagingSpec.TotalRecords = q2.Count();

			}
			else
			{

				pagingSpec.TotalRecords = MoviesWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff).GetRowCount();
				pagingSpec.Records = MoviesWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff).ToList();

			}

            return pagingSpec;
        }

        private SortBuilder<Movie> MoviesWhereCutoffUnmetQuery(PagingSpec<Movie> pagingSpec, List<QualitiesBelowCutoff> qualitiesBelowCutoff)
		{
            return Query.Join<Movie, MovieFile>(JoinType.Left, e => e.MovieFile, (e, s) => e.MovieFileId == s.Id)
                 .Where(pagingSpec.FilterExpression)
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
                result = Query.Where(movie => movie.CleanTitle == cleanTitleWithArabicNumbers).FirstWithYear(year) ??
                              Query.Where(movie => movie.CleanTitle == cleanTitleWithRomanNumbers).FirstWithYear(year);

                if (result == null)
                {
                    /*IEnumerable<Movie> movies = All();
                    Func<string, string> titleCleaner = title => CoreParser.CleanSeriesTitle(title.ToLower());
                    Func<IEnumerable<AlternativeTitle>, string, bool> altTitleComparer =
                        (alternativeTitles, atitle) =>
                        alternativeTitles.Any(altTitle => altTitle.CleanTitle == atitle);*/

                    /*result = movies.Where(m => altTitleComparer(m.AlternativeTitles, cleanTitle) ||
                                                altTitleComparer(m.AlternativeTitles, cleanTitleWithRomanNumbers) ||
                                          altTitleComparer(m.AlternativeTitles, cleanTitleWithArabicNumbers)).FirstWithYear(year);*/

                    //result = Query.Join<Movie, AlternativeTitle>(JoinType.Inner, m => m._newAltTitles,
                        //(m, t) => m.Id == t.MovieId && (t.CleanTitle == cleanTitle)).FirstWithYear(year);
                    result = Query.Where<AlternativeTitle>(t =>
                        t.CleanTitle == cleanTitle || t.CleanTitle == cleanTitleWithArabicNumbers
                        || t.CleanTitle == cleanTitleWithRomanNumbers).FirstWithYear(year);

                }
            }
            return result;
            /*return year.HasValue
                ? results?.FirstOrDefault(movie => movie.Year == year.Value)
             
             
              : results?.FirstOrDefault();*/
        }

        protected override QueryBuilder<Movie> AddJoinQueries(QueryBuilder<Movie> baseQuery)
        {
            baseQuery = base.AddJoinQueries(baseQuery);
            baseQuery = baseQuery.Join<Movie, AlternativeTitle>(JoinType.Left, m => m.AlternativeTitles,
                (m, t) => m.Id == t.MovieId);
            baseQuery = baseQuery.Join<Movie, MovieFile>(JoinType.Left, m => m.MovieFile, (m, f) => m.Id == f.MovieId);

            return baseQuery;
        }

        public Movie FindByTmdbId(int tmdbid)
        {
            return Query.Where(m => m.TmdbId == tmdbid).FirstOrDefault();
        }
    }
}
