using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Parser.RomanNumerals;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        Movie GetMovie(string title);
        RemoteMovie Map(ParsedMovieInfo parsedMovieInfo, string imdbId, int tmdbId, SearchCriteriaBase searchCriteria = null);
        RemoteMovie Map(ParsedMovieInfo parsedMovieInfo, int movieId);
        ParsedMovieInfo ParseMinimalPathMovieInfo(string path);
    }

    public class ParsingService : IParsingService
    {
        private static HashSet<ArabicRomanNumeral> _arabicRomanNumeralMappings;

        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public ParsingService(IMovieService movieService,
                              Logger logger)
        {
            _movieService = movieService;
            _logger = logger;

            if (_arabicRomanNumeralMappings == null)
            {
                _arabicRomanNumeralMappings = RomanNumeralParser.GetArabicRomanNumeralsMapping();
            }
        }

        public ParsedMovieInfo ParseMinimalPathMovieInfo(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = Parser.ParseMovieTitle(fileInfo.Name, true);

            if (result == null)
            {
                _logger.Debug("Attempting to parse movie info using directory and file names. {0}", fileInfo.Directory.Name);
                result = Parser.ParseMovieTitle(fileInfo.Directory.Name + " " + fileInfo.Name);
            }

            if (result == null)
            {
                _logger.Debug("Attempting to parse movie info using directory name. {0}", fileInfo.Directory.Name);
                result = Parser.ParseMovieTitle(fileInfo.Directory.Name + fileInfo.Extension);
            }

            return result;
        }

        public Movie GetMovie(string title)
        {
            var parsedMovieInfo = Parser.ParseMovieTitle(title);

            if (parsedMovieInfo == null)
            {
                return _movieService.FindByTitle(title);
            }

            var result = TryGetMovieByTitleAndOrYear(parsedMovieInfo);

            if (result != null)
            {
                return result.Movie;
            }

            return null;
        }

        public RemoteMovie Map(ParsedMovieInfo parsedMovieInfo, string imdbId, int tmdbId, SearchCriteriaBase searchCriteria = null)
        {
            return Map(parsedMovieInfo, imdbId, tmdbId, null, searchCriteria);
        }

        public RemoteMovie Map(ParsedMovieInfo parsedMovieInfo, int movieId)
        {
            return new RemoteMovie
            {
                ParsedMovieInfo = parsedMovieInfo,
                Movie = _movieService.GetMovie(movieId)
            };
        }

        public RemoteMovie Map(ParsedMovieInfo parsedMovieInfo, string imdbId, int tmdbId, Movie movie, SearchCriteriaBase searchCriteria)
        {
            var remoteMovie = new RemoteMovie
            {
                ParsedMovieInfo = parsedMovieInfo
            };

            if (movie == null)
            {
                var movieMatch = FindMovie(parsedMovieInfo, imdbId, tmdbId, searchCriteria);

                if (movieMatch != null)
                {
                    movie = movieMatch.Movie;
                    remoteMovie.MovieMatchType = movieMatch.MatchType;
                }
            }

            if (movie != null)
            {
                remoteMovie.Movie = movie;
            }

            remoteMovie.Languages = parsedMovieInfo.Languages;

            return remoteMovie;
        }

        private FindMovieResult FindMovie(ParsedMovieInfo parsedMovieInfo, string imdbId, int tmdbId, SearchCriteriaBase searchCriteria)
        {
            FindMovieResult result = null;

            if (!string.IsNullOrWhiteSpace(imdbId) && imdbId != "0")
            {
                result = TryGetMovieByImDbId(parsedMovieInfo, imdbId);
            }

            if (result == null && tmdbId > 0)
            {
                result = TryGetMovieByTmdbId(parsedMovieInfo, tmdbId);
            }

            if (result == null)
            {
                if (searchCriteria != null)
                {
                    result = TryGetMovieBySearchCriteria(parsedMovieInfo, imdbId, tmdbId, searchCriteria);
                }
                else
                {
                    result = TryGetMovieByTitleAndOrYear(parsedMovieInfo);
                }
            }

            if (result == null)
            {
                _logger.Debug($"No matching movie for titles {string.Join(", ", parsedMovieInfo.MovieTitles)} ({parsedMovieInfo.Year})");
            }

            return result;
        }

        private FindMovieResult TryGetMovieByImDbId(ParsedMovieInfo parsedMovieInfo, string imdbId)
        {
            var movie = _movieService.FindByImdbId(imdbId);

            // Should fix practically all problems, where indexer is shite at adding correct imdbids to movies.
            if (movie != null && (parsedMovieInfo.Year < 1800 || movie.MovieMetadata.Value.Year == parsedMovieInfo.Year || movie.MovieMetadata.Value.SecondaryYear == parsedMovieInfo.Year))
            {
                return new FindMovieResult(movie, MovieMatchType.Id);
            }

            return null;
        }

        private FindMovieResult TryGetMovieByTmdbId(ParsedMovieInfo parsedMovieInfo, int tmdbId)
        {
            var movie = _movieService.FindByTmdbId(tmdbId);

            // Should fix practically all problems, where indexer is shite at adding correct imdbids to movies.
            if (movie != null && (parsedMovieInfo.Year < 1800 || movie.MovieMetadata.Value.Year == parsedMovieInfo.Year || movie.MovieMetadata.Value.SecondaryYear == parsedMovieInfo.Year))
            {
                return new FindMovieResult(movie, MovieMatchType.Id);
            }

            return null;
        }

        private FindMovieResult TryGetMovieByTitleAndOrYear(ParsedMovieInfo parsedMovieInfo)
        {
            var candidates = _movieService.FindByTitleCandidates(parsedMovieInfo.MovieTitles, out var otherTitles);

            Movie movieByTitleAndOrYear;
            if (parsedMovieInfo.Year > 1800)
            {
                movieByTitleAndOrYear = _movieService.FindByTitle(parsedMovieInfo.MovieTitles, parsedMovieInfo.Year, otherTitles, candidates);
                if (movieByTitleAndOrYear != null)
                {
                    return new FindMovieResult(movieByTitleAndOrYear, MovieMatchType.Title);
                }

                // Only default to not using year when one is parsed if only one movie candidate exists
                if (candidates != null && candidates.Count == 1)
                {
                    movieByTitleAndOrYear = _movieService.FindByTitle(parsedMovieInfo.MovieTitles, null, otherTitles, candidates);
                    if (movieByTitleAndOrYear != null)
                    {
                        return new FindMovieResult(movieByTitleAndOrYear, MovieMatchType.Title);
                    }
                }

                return null;
            }

            movieByTitleAndOrYear = _movieService.FindByTitle(parsedMovieInfo.MovieTitles, null, otherTitles, candidates);
            if (movieByTitleAndOrYear != null)
            {
                return new FindMovieResult(movieByTitleAndOrYear, MovieMatchType.Title);
            }

            return null;
        }

        private FindMovieResult TryGetMovieBySearchCriteria(ParsedMovieInfo parsedMovieInfo, string imdbId, int tmdbId, SearchCriteriaBase searchCriteria)
        {
            Movie possibleMovie = null;

            var possibleTitles = new List<string>
            {
                searchCriteria.Movie.MovieMetadata.Value.CleanTitle
            };
            possibleTitles.AddIfNotNull(searchCriteria.Movie.MovieMetadata.Value.CleanOriginalTitle);
            possibleTitles.AddRange(searchCriteria.Movie.MovieMetadata.Value.AlternativeTitles.Select(t => t.CleanTitle));
            possibleTitles.AddRange(searchCriteria.Movie.MovieMetadata.Value.Translations.Select(t => t.CleanTitle));

            var cleanTitles = parsedMovieInfo.MovieTitles.Select(t => t.CleanMovieTitle()).ToArray();

            if (possibleTitles.Any(pt =>
                cleanTitles.Contains(pt)
                || _arabicRomanNumeralMappings.Any(mn =>
                    cleanTitles.Contains(pt.Replace(mn.ArabicNumeralAsString, mn.RomanNumeralLowerCase))
                    || cleanTitles.Any(t => t.Replace(mn.ArabicNumeralAsString, mn.RomanNumeralLowerCase) == pt))))
            {
                possibleMovie = searchCriteria.Movie;
            }

            if (possibleMovie != null)
            {
                if (parsedMovieInfo.Year < 1800 || possibleMovie.MovieMetadata.Value.Year == parsedMovieInfo.Year || possibleMovie.MovieMetadata.Value.SecondaryYear == parsedMovieInfo.Year)
                {
                    return new FindMovieResult(possibleMovie, MovieMatchType.Title);
                }
            }

            if (tmdbId > 0 && tmdbId == searchCriteria.Movie.TmdbId)
            {
                return new FindMovieResult(searchCriteria.Movie, MovieMatchType.Id);
            }

            if (imdbId.IsNotNullOrWhiteSpace() && imdbId == searchCriteria.Movie.ImdbId)
            {
                return new FindMovieResult(searchCriteria.Movie, MovieMatchType.Id);
            }

            return null;
        }
    }
}
