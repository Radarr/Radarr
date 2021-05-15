using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Augmenters;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Parser.RomanNumerals;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        Movie GetMovie(string title);
        MappingResult Map(ParsedMovieInfo parsedMovieInfo, string imdbId, SearchCriteriaBase searchCriteria = null);
        ParsedMovieInfo ParseMovieInfo(string title, List<object> helpers);
        ParsedMovieInfo EnhanceMovieInfo(ParsedMovieInfo parsedMovieInfo, List<object> helpers = null);
        ParsedMovieInfo ParseMinimalMovieInfo(string path, bool isDir = false);
        ParsedMovieInfo ParseMinimalPathMovieInfo(string path);
    }

    public class ParsingService : IParsingService
    {
        private static HashSet<ArabicRomanNumeral> _arabicRomanNumeralMappings;

        private readonly IMovieService _movieService;
        private readonly IConfigService _config;
        private readonly IEnumerable<IAugmentParsedMovieInfo> _augmenters;
        private readonly Logger _logger;

        public ParsingService(IMovieService movieService,
                              IConfigService configService,
                              IEnumerable<IAugmentParsedMovieInfo> augmenters,
                              Logger logger)
        {
            _movieService = movieService;
            _config = configService;
            _augmenters = augmenters;
            _logger = logger;

            if (_arabicRomanNumeralMappings == null)
            {
                _arabicRomanNumeralMappings = RomanNumeralParser.GetArabicRomanNumeralsMapping();
            }
        }

        public ParsedMovieInfo ParseMovieInfo(string title, List<object> helpers)
        {
            var result = Parser.ParseMovieTitle(title);

            if (result == null)
            {
                return null;
            }

            result = EnhanceMovieInfo(result, helpers);

            return result;
        }

        public ParsedMovieInfo EnhanceMovieInfo(ParsedMovieInfo minimalInfo, List<object> helpers = null)
        {
            if (helpers != null)
            {
                var augmenters = _augmenters.Where(a => helpers.Any(t => a.HelperType.IsInstanceOfType(t)) || a.HelperType == null);

                foreach (var augmenter in augmenters)
                {
                    minimalInfo = augmenter.AugmentMovieInfo(minimalInfo,
                        helpers.FirstOrDefault(h => augmenter.HelperType.IsInstanceOfType(h)));
                }
            }

            return minimalInfo;
        }

        public ParsedMovieInfo ParseMinimalMovieInfo(string file, bool isDir = false)
        {
            return Parser.ParseMovieTitle(file, isDir);
        }

        public ParsedMovieInfo ParseMinimalPathMovieInfo(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseMinimalMovieInfo(fileInfo.Name, true);

            if (result == null)
            {
                _logger.Debug("Attempting to parse movie info using directory and file names. {0}", fileInfo.Directory.Name);
                result = ParseMinimalMovieInfo(fileInfo.Directory.Name + " " + fileInfo.Name);
            }

            if (result == null)
            {
                _logger.Debug("Attempting to parse movie info using directory name. {0}", fileInfo.Directory.Name);
                result = ParseMinimalMovieInfo(fileInfo.Directory.Name + fileInfo.Extension);
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

            if (TryGetMovieByTitleAndOrYear(parsedMovieInfo, out var result) && result.MappingResultType == MappingResultType.Success)
            {
                return result.Movie;
            }

            return null;
        }

        public MappingResult Map(ParsedMovieInfo parsedMovieInfo, string imdbId, SearchCriteriaBase searchCriteria = null)
        {
            var result = GetMovie(parsedMovieInfo, imdbId, searchCriteria);

            if (result == null)
            {
                result = new MappingResult { MappingResultType = MappingResultType.Unknown };
                result.Movie = null;
            }

            //Use movie language as fallback if we could't parse a language (more accurate than just using English)
            if (parsedMovieInfo.Languages.Count <= 1 && parsedMovieInfo.Languages.First() == Language.Unknown && result.Movie != null)
            {
                parsedMovieInfo.Languages = new List<Language> { result.Movie.OriginalLanguage };
                _logger.Debug("Language couldn't be parsed from release, fallback to movie original language: {0}", result.Movie.OriginalLanguage.Name);
            }

            if (parsedMovieInfo.Languages.Contains(Language.Original))
            {
                parsedMovieInfo.Languages.Remove(Language.Original);
                if (!parsedMovieInfo.Languages.Contains(result.Movie.OriginalLanguage))
                {
                    parsedMovieInfo.Languages.Add(result.Movie.OriginalLanguage);
                }
            }

            result.RemoteMovie.ParsedMovieInfo = parsedMovieInfo;

            return result;
        }

        private MappingResult GetMovie(ParsedMovieInfo parsedMovieInfo, string imdbId, SearchCriteriaBase searchCriteria)
        {
            MappingResult result = null;

            if (!string.IsNullOrWhiteSpace(imdbId) && imdbId != "0")
            {
                if (TryGetMovieByImDbId(parsedMovieInfo, imdbId, out result))
                {
                    return result;
                }
            }

            if (searchCriteria != null)
            {
                if (TryGetMovieBySearchCriteria(parsedMovieInfo, searchCriteria, out result))
                {
                    return result;
                }
            }
            else
            {
                if (TryGetMovieByTitleAndOrYear(parsedMovieInfo, out result))
                {
                    return result;
                }
            }

            // nothing found up to here => logging that and returning null
            _logger.Debug($"No matching movie {parsedMovieInfo.MovieTitle}");
            return result;
        }

        private bool TryGetMovieByImDbId(ParsedMovieInfo parsedMovieInfo, string imdbId, out MappingResult result)
        {
            var movie = _movieService.FindByImdbId(imdbId);

            //Should fix practically all problems, where indexer is shite at adding correct imdbids to movies.
            if (movie != null && parsedMovieInfo.Year > 1800 && (parsedMovieInfo.Year != movie.Year && movie.SecondaryYear != parsedMovieInfo.Year))
            {
                result = new MappingResult { Movie = movie, MappingResultType = MappingResultType.WrongYear };
                return false;
            }

            if (movie != null)
            {
                result = new MappingResult { Movie = movie };
            }
            else
            {
                result = new MappingResult { Movie = movie, MappingResultType = MappingResultType.TitleNotFound };
            }

            return movie != null;
        }

        private bool TryGetMovieByTitleAndOrYear(ParsedMovieInfo parsedMovieInfo, out MappingResult result)
        {
            var candidates = _movieService.FindByTitleCandidates(parsedMovieInfo.MovieTitle, out var arabicTitle, out var romanTitle);

            Movie movieByTitleAndOrYear;
            if (parsedMovieInfo.Year > 1800)
            {
                movieByTitleAndOrYear = _movieService.FindByTitle(parsedMovieInfo.MovieTitle, parsedMovieInfo.Year, arabicTitle, romanTitle, candidates);
                if (movieByTitleAndOrYear != null)
                {
                    result = new MappingResult { Movie = movieByTitleAndOrYear };
                    return true;
                }

                // Only default to not using year when one is parsed if only one movie candidate exists
                if (candidates != null && candidates.Count == 1)
                {
                    movieByTitleAndOrYear = _movieService.FindByTitle(parsedMovieInfo.MovieTitle, null, arabicTitle, romanTitle, candidates);
                    if (movieByTitleAndOrYear != null)
                    {
                        result = new MappingResult { Movie = movieByTitleAndOrYear, MappingResultType = MappingResultType.WrongYear };
                        return false;
                    }
                }

                result = new MappingResult { Movie = movieByTitleAndOrYear, MappingResultType = MappingResultType.TitleNotFound };
                return false;
            }

            movieByTitleAndOrYear = _movieService.FindByTitle(parsedMovieInfo.MovieTitle, null, arabicTitle, romanTitle, candidates);
            if (movieByTitleAndOrYear != null)
            {
                result = new MappingResult { Movie = movieByTitleAndOrYear };
                return true;
            }

            result = new MappingResult { Movie = movieByTitleAndOrYear, MappingResultType = MappingResultType.TitleNotFound };
            return false;
        }

        private bool TryGetMovieBySearchCriteria(ParsedMovieInfo parsedMovieInfo, SearchCriteriaBase searchCriteria, out MappingResult result)
        {
            Movie possibleMovie = null;

            var possibleTitles = new List<string>();

            possibleTitles.Add(searchCriteria.Movie.CleanTitle);
            possibleTitles.AddRange(searchCriteria.Movie.AlternativeTitles.Select(t => t.CleanTitle));
            possibleTitles.AddRange(searchCriteria.Movie.Translations.Select(t => t.CleanTitle));

            var cleanTitle = parsedMovieInfo.MovieTitle.CleanMovieTitle();

            foreach (var title in possibleTitles)
            {
                if (title == cleanTitle)
                {
                    possibleMovie = searchCriteria.Movie;
                }

                foreach (var numeralMapping in _arabicRomanNumeralMappings)
                {
                    var arabicNumeral = numeralMapping.ArabicNumeralAsString;
                    var romanNumeral = numeralMapping.RomanNumeralLowerCase;

                    //_logger.Debug(cleanTitle);
                    if (title.Replace(arabicNumeral, romanNumeral) == cleanTitle)
                    {
                        possibleMovie = searchCriteria.Movie;
                    }

                    if (title == cleanTitle.Replace(arabicNumeral, romanNumeral))
                    {
                        possibleMovie = searchCriteria.Movie;
                    }
                }
            }

            if (possibleMovie != null)
            {
                if (parsedMovieInfo.Year < 1800 || possibleMovie.Year == parsedMovieInfo.Year || possibleMovie.SecondaryYear == parsedMovieInfo.Year)
                {
                    result = new MappingResult { Movie = possibleMovie, MappingResultType = MappingResultType.Success };
                    return true;
                }

                result = new MappingResult { Movie = possibleMovie, MappingResultType = MappingResultType.WrongYear };
                return false;
            }

            result = new MappingResult { Movie = searchCriteria.Movie, MappingResultType = MappingResultType.WrongTitle };

            return false;
        }
    }

    public class MappingResult
    {
        public string Message
        {
            get
            {
                switch (MappingResultType)
                {
                    case MappingResultType.Success:
                        return $"Successfully mapped release name {ReleaseName} to movie {Movie}";
                    case MappingResultType.NotParsable:
                        return $"Failed to find movie title in release name {ReleaseName}";
                    case MappingResultType.TitleNotFound:
                        return $"Could not find {RemoteMovie.ParsedMovieInfo.MovieTitle}";
                    case MappingResultType.WrongYear:
                        return $"Failed to map movie, expected year {RemoteMovie.Movie.Year}, but found {RemoteMovie.ParsedMovieInfo.Year}";
                    case MappingResultType.WrongTitle:
                        var comma = RemoteMovie.Movie.AlternativeTitles.Count > 0 ? ", " : "";
                        return
                            $"Failed to map movie, found title {RemoteMovie.ParsedMovieInfo.MovieTitle}, expected one of: {RemoteMovie.Movie.Title}{comma}{string.Join(", ", RemoteMovie.Movie.AlternativeTitles)}";
                    default:
                        return $"Failed to map movie for unknown reasons";
                }
            }
        }

        public RemoteMovie RemoteMovie;
        public MappingResultType MappingResultType { get; set; }
        public Movie Movie
        {
            get
            {
                return RemoteMovie.Movie;
            }
            set
            {
                ParsedMovieInfo parsedInfo = null;
                if (RemoteMovie != null)
                {
                    parsedInfo = RemoteMovie.ParsedMovieInfo;
                }

                RemoteMovie = new RemoteMovie
                {
                    Movie = value,
                    ParsedMovieInfo = parsedInfo
                };
            }
        }

        public string ReleaseName { get; set; }

        public override string ToString()
        {
            return string.Format(Message, RemoteMovie.Movie);
        }

        public Rejection ToRejection()
        {
            switch (MappingResultType)
            {
                case MappingResultType.Success:
                    return null;
                default:
                    return new Rejection(Message);
            }
        }
    }

    public enum MappingResultType
    {
        Unknown = -1,
        Success = 0,
        WrongYear = 2,
        WrongTitle = 3,
        TitleNotFound = 4,
        NotParsable = 5,
    }
}
