using System;
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
                _logger.Debug("Attempting to parse movie info using directory and file names. '{0}'", fileInfo.Directory.Name);
                result = Parser.ParseMovieTitle(fileInfo.Directory.Name + " " + fileInfo.Name);
            }

            if (result == null)
            {
                _logger.Debug("Attempting to parse movie info using directory name. '{0}'", fileInfo.Directory.Name);
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

            if (searchCriteria != null)
            {
                remoteMovie.MovieRequested = remoteMovie.Movie?.Id == searchCriteria.Movie?.Id;
            }

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
                _logger.Debug($"No matching movie for titles '{string.Join(", ", parsedMovieInfo.MovieTitles)} ({parsedMovieInfo.Year})'");
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

            if (IsTitleMatch(parsedMovieInfo.MovieTitles, searchCriteria.Movie))
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

            var matchesById = (tmdbId > 0 && tmdbId == searchCriteria.Movie.TmdbId) ||
                              (Parser.TryNormalizeImdbId(imdbId, out var normalizedImdbId) && normalizedImdbId == searchCriteria.Movie.ImdbId);

            if (!matchesById)
            {
                return null;
            }

            // Indexers sometimes attach the searched movie's ID to a different movie (a remake, the original or an unrelated title),
            // so only trust the ID when the release year is close to the movie's year
            if (!IsWithinOneYear(parsedMovieInfo.Year, searchCriteria.Movie) && !IsYearPartOfTitle(parsedMovieInfo, searchCriteria.Movie))
            {
                _logger.Debug("Release matches {0} by ID, but its year {1} is more than one year off, not matching by ID", searchCriteria.Movie, parsedMovieInfo.Year);

                return null;
            }

            return new FindMovieResult(searchCriteria.Movie, MovieMatchType.Id);
        }

        private static bool IsTitleMatch(IEnumerable<string> titles, Movie movie)
        {
            var possibleTitles = new List<string>
            {
                movie.MovieMetadata.Value.CleanTitle
            };
            possibleTitles.AddIfNotNull(movie.MovieMetadata.Value.CleanOriginalTitle);
            possibleTitles.AddRange(movie.MovieMetadata.Value.AlternativeTitles.Select(t => t.CleanTitle));
            possibleTitles.AddRange(movie.MovieMetadata.Value.Translations.Select(t => t.CleanTitle));

            var cleanTitles = titles.Select(t => t.CleanMovieTitle()).ToArray();

            return possibleTitles.Any(pt =>
                cleanTitles.Contains(pt)
                || _arabicRomanNumeralMappings.Any(mn =>
                    cleanTitles.Contains(pt.Replace(mn.ArabicNumeralAsString, mn.RomanNumeralLowerCase))
                    || cleanTitles.Any(t => t.Replace(mn.ArabicNumeralAsString, mn.RomanNumeralLowerCase) == pt)));
        }

        // The parser takes a number at the end of a title for the year when the release leaves out the real year:
        // Blade.Runner.2049.1080p parses as "Blade Runner" (2049)
        private static bool IsYearPartOfTitle(ParsedMovieInfo parsedMovieInfo, Movie movie)
        {
            return parsedMovieInfo.Year > 1800 && IsTitleMatch(parsedMovieInfo.MovieTitles.Select(t => $"{t} {parsedMovieInfo.Year}"), movie);
        }

        private static bool IsWithinOneYear(int year, Movie movie)
        {
            var movieMetadata = movie.MovieMetadata.Value;

            if (movieMetadata.Year < 1800)
            {
                return true;
            }

            return Math.Abs(movieMetadata.Year - year) <= 1 ||
                   (movieMetadata.SecondaryYear.HasValue && Math.Abs(movieMetadata.SecondaryYear.Value - year) <= 1);
        }
    }
}
