﻿using System;
using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Datastore.Extensions;
using Marr.Data.QGen;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;

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
        private readonly Dictionary<string, string> romanNumeralsMapper = new Dictionary<string, string>
        {
            { "1", "I"},
            { "2", "II"},
            { "3", "III"},
            { "4", "IV"},
            { "5", "V"},
            { "6", "VI"},
            { "7", "VII"},
            { "8", "VII"},
            { "9", "IX"},
            { "10", "X"},

        }; //If a movie has more than 10 parts fuck 'em.

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
            cleanTitle = cleanTitle.ToLowerInvariant();

            var cleanRoman = cleanTitle;

            var cleanNum = cleanTitle;

            foreach (KeyValuePair<string, string> entry in romanNumeralsMapper)
            {
                string num = entry.Key;
                string roman = entry.Value.ToLower();

                cleanRoman = cleanRoman.Replace(num, roman);

                cleanNum = cleanNum.Replace(roman, num);
            }

            var result = Query.Where(s => s.CleanTitle == cleanTitle).FirstOrDefault();

            if (result == null)
            {
                result = Query.Where(s => s.CleanTitle == cleanNum).OrWhere(s => s.CleanTitle == cleanRoman).FirstOrDefault();

                if (result == null)
                {
                    var movies = this.All();

                    result = movies.Where(m => m.AlternativeTitles.Any(t => Parser.Parser.CleanSeriesTitle(t.ToLower()) == cleanTitle ||
                    Parser.Parser.CleanSeriesTitle(t.ToLower()) == cleanRoman ||
                    Parser.Parser.CleanSeriesTitle(t.ToLower()) == cleanNum)).FirstOrDefault();

                    return result;
                }
                else
                {
                    return result;
                }

            }
            else
            {
                return result;
            }
        }

        public Movie FindByTitle(string cleanTitle, int year)
        {
            cleanTitle = cleanTitle.ToLowerInvariant();

            var cleanRoman = cleanTitle;

            var cleanNum = cleanTitle;

            foreach (KeyValuePair<string, string> entry in romanNumeralsMapper)
            {
                string num = entry.Key;
                string roman = entry.Value.ToLower();

                cleanRoman = cleanRoman.Replace(num, roman);

                cleanNum = cleanNum.Replace(roman, num);
            }

            var results = Query.Where(s => s.CleanTitle == cleanTitle);

            if (results == null)
            {
                results = Query.Where(s => s.CleanTitle == cleanNum).OrWhere(s => s.CleanTitle == cleanRoman);

                if (results == null)
                {
                    var movies = this.All();

                    var listResults = movies.Where(m => m.AlternativeTitles.Any(t => Parser.Parser.CleanSeriesTitle(t.ToLower()) == cleanTitle ||
                    Parser.Parser.CleanSeriesTitle(t.ToLower()) == cleanRoman ||
                    Parser.Parser.CleanSeriesTitle(t.ToLower()) == cleanNum));

                    return listResults.Where(m => m.Year == year).FirstOrDefault();
                }
                else
                {
                    return results.Where(m => m.Year == year).FirstOrDefault();
                }

            }
            else
            {
                return results.Where(m => m.Year == year).FirstOrDefault();
            }
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
			}

			if (pagingSpec.Records.Count == 0 && pagingSpec.Page != 1)
			{
				var lastPossiblePage = pagingSpec.TotalRecords / pagingSpec.PageSize + 1;
				pagingSpec.Page = lastPossiblePage;
				return GetPaged(pagingSpec);
			}

			return pagingSpec;
		}

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

            pagingSpec.TotalRecords = MoviesWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff).GetRowCount();
            pagingSpec.Records = MoviesWhereCutoffUnmetQuery(pagingSpec, qualitiesBelowCutoff).ToList();

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
                    clauses.Add(string.Format("([t0].[ProfileId] = {0} AND [t1].[Quality] LIKE '%_quality_: {1},%')", profile.ProfileId, belowCutoff));
                }
            }

            return string.Format("({0})", string.Join(" OR ", clauses));
        }

        public Movie FindByTmdbId(int tmdbid)
        {
            return Query.Where(m => m.TmdbId == tmdbid).FirstOrDefault();
        }
    }
}
