using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies.AlternativeTitles;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Parser.RomanNumerals;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Augmenters;

namespace NzbDrone.Core.Parser
{
    public interface IParsingService
    {
        LocalMovie GetLocalMovie(string filename, ParsedMovieInfo minimalInfo, Movie movie, List<object> helpers, bool sceneSource = false);
        Movie GetMovie(string title);
        MappingResult Map(ParsedMovieInfo parsedMovieInfo, string imdbId, SearchCriteriaBase searchCriteria = null);
        ParsedMovieInfo ParseMovieInfo(string title, List<object> helpers);
        ParsedMovieInfo ParseMoviePathInfo(string path, List<object> helpers);
        ParsedMovieInfo ParseMinimalMovieInfo(string path);
        ParsedMovieInfo ParseMinimalPathMovieInfo(string path);
        List<CustomFormat> ParseCustomFormat(ParsedMovieInfo movieInfo);
        List<FormatTagMatchResult> MatchFormatTags(ParsedMovieInfo movieInfo);
    }

    public class ParsingService : IParsingService
    {
        private readonly IMovieService _movieService;
        private readonly IConfigService _config;
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly ICustomFormatService _formatService;
        private readonly IEnumerable<IAugmentParsedMovieInfo> _augmenters;
        private readonly Logger _logger;
        private static HashSet<ArabicRomanNumeral> _arabicRomanNumeralMappings;

        public ParsingService(
                              IMovieService movieService,
                              IConfigService configService,
                              IQualityDefinitionService qualityDefinitionService,
                              ICustomFormatService formatService,
                              IEnumerable<IAugmentParsedMovieInfo> augmenters,
                              Logger logger)
        {
            _movieService = movieService;
            _config = configService;
            _qualityDefinitionService = qualityDefinitionService;
            _formatService = formatService;
            _augmenters = augmenters;
            _logger = logger;

            if (_arabicRomanNumeralMappings == null)
            {
                _arabicRomanNumeralMappings = RomanNumeralParser.GetArabicRomanNumeralsMapping();
            }
        }

        public ParsedMovieInfo ParseMovieInfo(string title, List<object> helpers)
        {
            var result = Parser.ParseMovieTitle(title, _config.ParsingLeniency > 0);
            if (result == null)
            {
                return null;
            }

            result = EnhanceMinimalInfo(result, helpers);

            return result;
        }

        private ParsedMovieInfo EnhanceMinimalInfo(ParsedMovieInfo minimalInfo, List<object> helpers)
        {
            minimalInfo.Languages = LanguageParser.ParseLanguages(minimalInfo.SimpleReleaseTitle);
            _logger.Debug("Language(s) parsed: {0}", string.Join(", ", minimalInfo.Languages.ToExtendedString()));

            minimalInfo.Quality = QualityParser.ParseQuality(minimalInfo.SimpleReleaseTitle);

            if (minimalInfo.Edition.IsNullOrWhiteSpace())
            {
                minimalInfo.Edition = Parser.ParseEdition(minimalInfo.SimpleReleaseTitle);
            }

            minimalInfo.ReleaseGroup = Parser.ParseReleaseGroup(minimalInfo.SimpleReleaseTitle);

            minimalInfo.ImdbId = Parser.ParseImdbId(minimalInfo.SimpleReleaseTitle);

            minimalInfo = AugmentMovieInfo(minimalInfo, helpers);

            // After the augmenters have done their job on languages we can do our static method as well.
            minimalInfo.Languages =
                LanguageParser.EnhanceLanguages(minimalInfo.SimpleReleaseTitle, minimalInfo.Languages);

            minimalInfo.Quality.Quality = Quality.FindByInfo(minimalInfo.Quality.Source, minimalInfo.Quality.Resolution,
                minimalInfo.Quality.Modifier);

            minimalInfo.Quality.CustomFormats = ParseCustomFormat(minimalInfo);

            _logger.Debug("Quality parsed: {0}", minimalInfo.Quality);

            return minimalInfo;
        }

        private ParsedMovieInfo AugmentMovieInfo(ParsedMovieInfo minimalInfo, List<object> helpers)
        {
            var augmenters = _augmenters.Where(a => helpers.Any(t => a.HelperType.IsInstanceOfType(t)) || a.HelperType == null);

            foreach (var augmenter in augmenters)
            {
                minimalInfo = augmenter.AugmentMovieInfo(minimalInfo,
                    helpers.FirstOrDefault(h => augmenter.HelperType.IsInstanceOfType(h)));
            }

            return minimalInfo;
        }

        public ParsedMovieInfo ParseMoviePathInfo(string path, List<object> helpers)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseMovieInfo(fileInfo.Name, helpers);

            if (result == null)
            {
                _logger.Debug("Attempting to parse movie info using directory and file names. {0}", fileInfo.Directory.Name);
                result = ParseMovieInfo(fileInfo.Directory.Name + " " + fileInfo.Name, helpers);
            }

            if (result == null)
            {
                _logger.Debug("Attempting to parse movie info using directory name. {0}", fileInfo.Directory.Name);
                result = ParseMovieInfo(fileInfo.Directory.Name + fileInfo.Extension, helpers);
            }

            return result;
        }

        public List<CustomFormat> ParseCustomFormat(ParsedMovieInfo movieInfo)
        {
            var matches = MatchFormatTags(movieInfo);
            var goodMatches = matches.Where(m => m.GoodMatch);
            return goodMatches.Select(r => r.CustomFormat).ToList();
        }

        public List<FormatTagMatchResult> MatchFormatTags(ParsedMovieInfo movieInfo)
        {
            var formats = _formatService.All();

            if (movieInfo.ExtraInfo.GetValueOrDefault("AdditionalFormats") is List<CustomFormat> additionalFormats)
            {
                formats.AddRange(additionalFormats);
            }

            var matches = new List<FormatTagMatchResult>();

            foreach (var customFormat in formats)
            {
                var formatMatches = customFormat.FormatTags.GroupBy(t => t.TagType).Select(g =>
                    new FormatTagMatchesGroup(g.Key, g.ToList().ToDictionary(t => t, t => t.DoesItMatch(movieInfo))));

                var formatTagMatchesGroups = formatMatches.ToList();
                matches.Add(new FormatTagMatchResult
                {
                    CustomFormat = customFormat,
                    GroupMatches = formatTagMatchesGroups,
                    GoodMatch = formatTagMatchesGroups.All(g => g.DidMatch)
                });
            }

            return matches;
        }

        public LocalMovie GetLocalMovie(string filename, ParsedMovieInfo minimalInfo, Movie movie, List<object> helpers, bool sceneSource = false)
        {
            var enhanced = EnhanceMinimalInfo(minimalInfo, helpers);

            return new LocalMovie
            {
                Movie = movie,
                Quality = enhanced.Quality,
                Path = filename,
                ParsedMovieInfo = enhanced,
                ExistingFile = movie.Path.IsParentPath(filename),
                MediaInfo = helpers.FirstOrDefault(h => h?.GetType() == typeof(MediaInfoModel)) as MediaInfoModel
            };
        }

        public ParsedMovieInfo ParseMinimalMovieInfo(string file)
        {
            return Parser.ParseMovieTitle(file, _config.ParsingLeniency > 0);
        }

        public ParsedMovieInfo ParseMinimalPathMovieInfo(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseMinimalMovieInfo(fileInfo.Name);

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
            var parsedMovieInfo = Parser.ParseMovieTitle(title, _config.ParsingLeniency > 0);

            if (parsedMovieInfo == null)
            {
                return _movieService.FindByTitle(title);
            }

            var movies = _movieService.FindByTitle(parsedMovieInfo.MovieTitle, parsedMovieInfo.Year);

            if (movies == null)
            {
                movies = _movieService.FindByTitle(parsedMovieInfo.MovieTitle.Replace("DC", "").Trim());
            }

            return movies;
        }

        public MappingResult Map(ParsedMovieInfo parsedMovieInfo, string imdbId, SearchCriteriaBase searchCriteria = null)
        {
            var result = GetMovie(parsedMovieInfo, imdbId, searchCriteria);

            if (result == null) {
                result = new MappingResult {MappingResultType = MappingResultType.Unknown};
                result.Movie = null;
            }

            result.RemoteMovie.ParsedMovieInfo = parsedMovieInfo;

            return result;
        }

        private MappingResult GetMovie(ParsedMovieInfo parsedMovieInfo, string imdbId, SearchCriteriaBase searchCriteria)
        {
            // TODO: Answer me this: Wouldn't it be smarter to start out looking for a movie if we have an ImDb Id?
            MappingResult result = null;
            if (!String.IsNullOrWhiteSpace(imdbId) && imdbId != "0")
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
                TryGetMovieByTitleAndOrYear(parsedMovieInfo, out result);
                return result;
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
                result = new MappingResult { Movie = movie, MappingResultType = MappingResultType.WrongYear};
                return false;
            }
            if (movie != null) {
                result = new MappingResult { Movie = movie };
            } else {
                result = new MappingResult { Movie = movie, MappingResultType = MappingResultType.TitleNotFound};
            }
            return movie != null;
        }

        private bool TryGetMovieByTitleAndOrYear(ParsedMovieInfo parsedMovieInfo, out MappingResult result)
        {
            Func<Movie, bool> isNotNull = movie => movie != null;
            Movie movieByTitleAndOrYear = null;

            if (parsedMovieInfo.Year > 1800)
            {
                movieByTitleAndOrYear = _movieService.FindByTitle(parsedMovieInfo.MovieTitle, parsedMovieInfo.Year);
                if (isNotNull(movieByTitleAndOrYear))
                {
                    result = new MappingResult { Movie = movieByTitleAndOrYear };
                    return true;
                }

                movieByTitleAndOrYear = _movieService.FindByTitle(parsedMovieInfo.MovieTitle);
                if (isNotNull(movieByTitleAndOrYear))
                {
                    result = new MappingResult { Movie = movieByTitleAndOrYear, MappingResultType = MappingResultType.WrongYear};
                    return false;
                }

                if (_config.ParsingLeniency == ParsingLeniencyType.MappingLenient)
                {
                    movieByTitleAndOrYear = _movieService.FindByTitleInexact(parsedMovieInfo.MovieTitle, parsedMovieInfo.Year);
                    if (isNotNull(movieByTitleAndOrYear))
                    {
                        result = new MappingResult {Movie = movieByTitleAndOrYear, MappingResultType = MappingResultType.SuccessLenientMapping};
                        return true;
                    }
                }

                result = new MappingResult { Movie = movieByTitleAndOrYear, MappingResultType = MappingResultType.TitleNotFound};
                return false;
            }

            movieByTitleAndOrYear = _movieService.FindByTitle(parsedMovieInfo.MovieTitle);
            if (isNotNull(movieByTitleAndOrYear))
            {
                result = new MappingResult { Movie = movieByTitleAndOrYear };
                return true;
            }

            if (_config.ParsingLeniency == ParsingLeniencyType.MappingLenient)
            {
                movieByTitleAndOrYear = _movieService.FindByTitleInexact(parsedMovieInfo.MovieTitle, null);
                if (isNotNull(movieByTitleAndOrYear))
                {
                    result = new MappingResult {Movie = movieByTitleAndOrYear, MappingResultType = MappingResultType.SuccessLenientMapping};
                    return true;
                }
            }

            result = new MappingResult { Movie = movieByTitleAndOrYear, MappingResultType = MappingResultType.TitleNotFound};
            return false;
        }

        private bool TryGetMovieBySearchCriteria(ParsedMovieInfo parsedMovieInfo, SearchCriteriaBase searchCriteria, out MappingResult result)
        {
            Movie possibleMovie = null;

            List<string> possibleTitles = new List<string>();

            possibleTitles.Add(searchCriteria.Movie.CleanTitle);

            foreach (AlternativeTitle altTitle in searchCriteria.Movie.AlternativeTitles)
            {
                possibleTitles.Add(altTitle.CleanTitle);
            }

            string cleanTitle = parsedMovieInfo.MovieTitle.CleanSeriesTitle();

            foreach (string title in possibleTitles)
            {
                if (title == parsedMovieInfo.MovieTitle.CleanSeriesTitle())
                {
                    possibleMovie = searchCriteria.Movie;
                }

                foreach (ArabicRomanNumeral numeralMapping in _arabicRomanNumeralMappings)
                {
                    string arabicNumeral = numeralMapping.ArabicNumeralAsString;
                    string romanNumeral = numeralMapping.RomanNumeralLowerCase;

                    //_logger.Debug(cleanTitle);

                    if (title.Replace(arabicNumeral, romanNumeral) == parsedMovieInfo.MovieTitle.CleanSeriesTitle())
                    {
                        possibleMovie = searchCriteria.Movie;
                    }

                    if (title == parsedMovieInfo.MovieTitle.CleanSeriesTitle().Replace(arabicNumeral, romanNumeral))
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

            if (_config.ParsingLeniency == ParsingLeniencyType.MappingLenient)
            {
                if (searchCriteria.Movie.CleanTitle.Contains(cleanTitle) ||
                    cleanTitle.Contains(searchCriteria.Movie.CleanTitle))
                {
                    possibleMovie = searchCriteria.Movie;
                    if (parsedMovieInfo.Year > 1800 && parsedMovieInfo.Year == possibleMovie.Year || possibleMovie.SecondaryYear == parsedMovieInfo.Year)
                    {
                        result = new MappingResult {Movie = possibleMovie, MappingResultType = MappingResultType.SuccessLenientMapping};
                        return true;
                    }

                    if (parsedMovieInfo.Year < 1800)
                    {
                        result = new MappingResult { Movie = possibleMovie, MappingResultType = MappingResultType.SuccessLenientMapping };
                        return true;
                    }

                    result = new MappingResult { Movie = possibleMovie, MappingResultType = MappingResultType.WrongYear };
                    return false;
                }
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
                        break;
                    case MappingResultType.SuccessLenientMapping:
                        return $"Successfully mapped parts of the release name {ReleaseName} to movie {Movie}";
                        break;
                    case MappingResultType.NotParsable:
                        return $"Failed to find movie title in release name {ReleaseName}";
                        break;
                    case MappingResultType.TitleNotFound:
                        return $"Could not find {RemoteMovie.ParsedMovieInfo.MovieTitle}";
                        break;
                    case MappingResultType.WrongYear:
                        return $"Failed to map movie, expected year {RemoteMovie.Movie.Year}, but found {RemoteMovie.ParsedMovieInfo.Year}";
                    case MappingResultType.WrongTitle:
                        var comma = RemoteMovie.Movie.AlternativeTitles.Count > 0 ? ", " : "";
                        return
                            $"Failed to map movie, found title {RemoteMovie.ParsedMovieInfo.MovieTitle}, expected one of: {RemoteMovie.Movie.Title}{comma}{string.Join(", ", RemoteMovie.Movie.AlternativeTitles)}";
                    default:
                        return $"Failed to map movie for unkown reasons";
                }
            }
        }

        public RemoteMovie RemoteMovie;
        public MappingResultType MappingResultType { get; set; }
        public Movie Movie {
            get {
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

        public override string ToString() {
            return string.Format(Message, RemoteMovie.Movie);
        }

        public Rejection ToRejection() {
            switch (MappingResultType)
            {
                case MappingResultType.Success:
                case MappingResultType.SuccessLenientMapping:
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
        SuccessLenientMapping = 1,
        WrongYear = 2,
        WrongTitle = 3,
        TitleNotFound = 4,
        NotParsable = 5,
    }
}
