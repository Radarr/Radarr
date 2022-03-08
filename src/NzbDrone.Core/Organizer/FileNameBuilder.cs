using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Translations;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildFileName(Movie movie, MovieFile movieFile, NamingConfig namingConfig = null, List<CustomFormat> customFormats = null);
        string BuildFilePath(Movie movie, string fileName, string extension);
        BasicNamingConfig GetBasicNamingConfig(NamingConfig nameSpec);
        string GetMovieFolder(Movie movie, NamingConfig namingConfig = null);
    }

    public class FileNameBuilder : IBuildFileNames
    {
        private const string MediaInfoVideoDynamicRangeToken = "{MediaInfo VideoDynamicRange}";
        private const string MediaInfoVideoDynamicRangeTypeToken = "{MediaInfo VideoDynamicRangeType}";

        private readonly INamingConfigService _namingConfigService;
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly IUpdateMediaInfo _mediaInfoUpdater;
        private readonly IMovieTranslationService _movieTranslationService;
        private readonly ICustomFormatService _formatService;
        private readonly Logger _logger;

        private static readonly Regex TitleRegex = new Regex(@"\{(?<prefix>[- ._\[(]*)(?<token>(?:[a-z0-9]+)(?:(?<separator>[- ._]+)(?:[a-z0-9]+))?)(?::(?<customFormat>[a-z0-9|]+))?(?<suffix>[- ._)\]]*)\}",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private static readonly Regex TagsRegex = new Regex(@"(?<tags>\{tags(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SeasonEpisodePatternRegex = new Regex(@"(?<separator>(?<=})[- ._]+?)?(?<seasonEpisode>s?{season(?:\:0+)?}(?<episodeSeparator>[- ._]?[ex])(?<episode>{episode(?:\:0+)?}))(?<separator>[- ._]+?(?={))?",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex AbsoluteEpisodePatternRegex = new Regex(@"(?<separator>(?<=})[- ._]+?)?(?<absolute>{absolute(?:\:0+)?})(?<separator>[- ._]+?(?={))?",
                                                                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex AirDateRegex = new Regex(@"\{Air(\s|\W|_)Date\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex MovieTitleRegex = new Regex(@"(?<token>\{((?:(Movie|Original))(?<separator>[- ._])(Clean|Original)?(Title|Filename)(The)?)(?::(?<customFormat>[a-z0-9|]+))?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FileNameCleanupRegex = new Regex(@"([- ._])(\1)+", RegexOptions.Compiled);
        private static readonly Regex TrimSeparatorsRegex = new Regex(@"[- ._]$", RegexOptions.Compiled);

        private static readonly Regex ScenifyRemoveChars = new Regex(@"(?<=\s)(,|<|>|\/|\\|;|:|'|""|\||`|~|!|\?|@|$|%|^|\*|-|_|=){1}(?=\s)|('|:|\?|,)(?=(?:(?:s|m)\s)|\s|$)|(\(|\)|\[|\]|\{|\})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ScenifyReplaceChars = new Regex(@"[\/]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //TODO: Support Written numbers (One, Two, etc) and Roman Numerals (I, II, III etc)
        private static readonly Regex MultiPartCleanupRegex = new Regex(@"(?:\(\d+\)|(Part|Pt\.?)\s?\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly char[] EpisodeTitleTrimCharacters = new[] { ' ', '.', '?' };

        private static readonly Regex TitlePrefixRegex = new Regex(@"^(The|An|A) (.*?)((?: *\([^)]+\))*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ReservedDeviceNamesRegex = new Regex(@"^(?:aux|com[1-9]|con|lpt[1-9]|nul|prn)\.", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // generated from https://www.loc.gov/standards/iso639-2/ISO-639-2_utf-8.txt
        public static readonly ImmutableDictionary<string, string> Iso639BTMap = new Dictionary<string, string>
        {
            { "alb", "sqi" },
            { "arm", "hye" },
            { "baq", "eus" },
            { "bur", "mya" },
            { "chi", "zho" },
            { "cze", "ces" },
            { "dut", "nld" },
            { "fre", "fra" },
            { "geo", "kat" },
            { "ger", "deu" },
            { "gre", "ell" },
            { "ice", "isl" },
            { "mac", "mkd" },
            { "mao", "mri" },
            { "may", "msa" },
            { "per", "fas" },
            { "rum", "ron" },
            { "slo", "slk" },
            { "tib", "bod" },
            { "wel", "cym" }
        }.ToImmutableDictionary();

        public FileNameBuilder(INamingConfigService namingConfigService,
                               IQualityDefinitionService qualityDefinitionService,
                               IUpdateMediaInfo mediaInfoUpdater,
                               IMovieTranslationService movieTranslationService,
                               ICustomFormatService formatService,
                               Logger logger)
        {
            _namingConfigService = namingConfigService;
            _qualityDefinitionService = qualityDefinitionService;
            _mediaInfoUpdater = mediaInfoUpdater;
            _movieTranslationService = movieTranslationService;
            _formatService = formatService;
            _logger = logger;
        }

        public string BuildFileName(Movie movie, MovieFile movieFile, NamingConfig namingConfig = null, List<CustomFormat> customFormats = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            if (!namingConfig.RenameMovies)
            {
                return GetOriginalTitle(movieFile);
            }

            var pattern = namingConfig.StandardMovieFormat;
            var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);

            UpdateMediaInfoIfNeeded(pattern, movieFile, movie);

            AddMovieTokens(tokenHandlers, movie);
            AddReleaseDateTokens(tokenHandlers, movie.Year);
            AddIdTokens(tokenHandlers, movie);
            AddQualityTokens(tokenHandlers, movie, movieFile);
            AddMediaInfoTokens(tokenHandlers, movieFile);
            AddMovieFileTokens(tokenHandlers, movieFile);
            AddTagsTokens(tokenHandlers, movieFile);
            AddCustomFormats(tokenHandlers, movie, movieFile, customFormats);

            var splitPatterns = pattern.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var components = new List<string>();

            foreach (var s in splitPatterns)
            {
                var splitPattern = s;

                var component = ReplaceTokens(splitPattern, tokenHandlers, namingConfig).Trim();

                component = FileNameCleanupRegex.Replace(component, match => match.Captures[0].Value[0].ToString());
                component = TrimSeparatorsRegex.Replace(component, string.Empty);
                component = ReplaceReservedDeviceNames(component);

                if (component.IsNotNullOrWhiteSpace())
                {
                    components.Add(component);
                }
            }

            return Path.Combine(components.ToArray());
        }

        public string BuildFilePath(Movie movie, string fileName, string extension)
        {
            Ensure.That(extension, () => extension).IsNotNullOrWhiteSpace();

            var path = movie.Path;

            return Path.Combine(path, fileName + extension);
        }

        public BasicNamingConfig GetBasicNamingConfig(NamingConfig nameSpec)
        {
            return new BasicNamingConfig(); //For now let's be lazy
        }

        public string GetMovieFolder(Movie movie, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            var movieFile = movie.MovieFile;

            var pattern = namingConfig.MovieFolderFormat;
            var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);

            AddMovieTokens(tokenHandlers, movie);
            AddReleaseDateTokens(tokenHandlers, movie.Year);
            AddIdTokens(tokenHandlers, movie);

            if (movie.MovieFile != null)
            {
                AddQualityTokens(tokenHandlers, movie, movieFile);
                AddMediaInfoTokens(tokenHandlers, movieFile);
                AddMovieFileTokens(tokenHandlers, movieFile);
                AddTagsTokens(tokenHandlers, movieFile);
            }
            else
            {
                AddMovieFileTokens(tokenHandlers, new MovieFile { SceneName = $"{movie.Title} {movie.Year}", RelativePath = $"{movie.Title} {movie.Year}" });
            }

            var splitPatterns = pattern.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            var components = new List<string>();

            foreach (var s in splitPatterns)
            {
                var splitPattern = s;

                var component = ReplaceTokens(splitPattern, tokenHandlers, namingConfig);
                component = CleanFolderName(component);
                component = ReplaceReservedDeviceNames(component);

                if (component.IsNotNullOrWhiteSpace())
                {
                    components.Add(component);
                }
            }

            return Path.Combine(components.ToArray());
        }

        public static string CleanTitle(string title)
        {
            title = title.Replace("&", "and");
            title = ScenifyReplaceChars.Replace(title, " ");
            title = ScenifyRemoveChars.Replace(title, string.Empty);

            return title;
        }

        public static string TitleThe(string title)
        {
            return TitlePrefixRegex.Replace(title, "$2, $1$3");
        }

        public static string CleanFileName(string name, bool replace = true, ColonReplacementFormat colonReplacement = ColonReplacementFormat.Delete)
        {
            var colonReplacementFormat = colonReplacement.GetFormatString();

            string result = name;
            string[] badCharacters = { "\\", "/", "<", ">", "?", "*", ":", "|", "\"" };
            string[] goodCharacters = { "+", "+", "", "", "!", "-", colonReplacementFormat, "", "" };

            for (int i = 0; i < badCharacters.Length; i++)
            {
                result = result.Replace(badCharacters[i], replace ? goodCharacters[i] : string.Empty);
            }

            return result.Trim();
        }

        public static string CleanFolderName(string name)
        {
            name = FileNameCleanupRegex.Replace(name, match => match.Captures[0].Value[0].ToString());

            return name.Trim(' ', '.');
        }

        private void AddMovieTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie)
        {
            tokenHandlers["{Movie Title}"] = m => GetLanguageTitle(movie, m.CustomFormat);
            tokenHandlers["{Movie CleanTitle}"] = m => CleanTitle(GetLanguageTitle(movie, m.CustomFormat));
            tokenHandlers["{Movie Title The}"] = m => TitleThe(movie.Title);
            tokenHandlers["{Movie TitleFirstCharacter}"] = m => TitleThe(movie.Title).Substring(0, 1).FirstCharToUpper();
            tokenHandlers["{Movie OriginalTitle}"] = m => movie.MovieMetadata.Value.OriginalTitle ?? string.Empty;
            tokenHandlers["{Movie CleanOriginalTitle}"] = m => CleanTitle(movie.MovieMetadata.Value.OriginalTitle) ?? string.Empty;

            tokenHandlers["{Movie Certification}"] = m => movie.MovieMetadata.Value.Certification ?? string.Empty;
            tokenHandlers["{Movie Collection}"] = m => movie.MovieMetadata.Value.CollectionTitle ?? string.Empty;
        }

        private string GetLanguageTitle(Movie movie, string isoCodes)
        {
            if (isoCodes.IsNotNullOrWhiteSpace())
            {
                foreach (var isoCode in isoCodes.Split('|'))
                {
                    var language = IsoLanguages.Find(isoCode.ToLower())?.Language;

                    if (language == null)
                    {
                        continue;
                    }

                    var titles = movie.MovieMetadata.Value.Translations.Where(t => t.Language == language).ToList();

                    if (!movie.MovieMetadata.Value.Translations.Any())
                    {
                        titles = _movieTranslationService.GetAllTranslationsForMovieMetadata(movie.MovieMetadataId).Where(t => t.Language == language).ToList();
                    }

                    return titles.FirstOrDefault()?.Title ?? movie.Title;
                }
            }

            return movie.Title;
        }

        private void AddTagsTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, MovieFile movieFile)
        {
            if (movieFile.Edition.IsNotNullOrWhiteSpace())
            {
                tokenHandlers["{Edition Tags}"] = m => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(movieFile.Edition.ToLower());
            }
        }

        private void AddReleaseDateTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, int releaseYear)
        {
            if (releaseYear == 0)
            {
                tokenHandlers["{Release Year}"] = m => string.Empty;
                return;
            }

            tokenHandlers["{Release Year}"] = m => string.Format("{0}", releaseYear.ToString()); //Do I need m.CustomFormat?
        }

        private void AddIdTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie)
        {
            tokenHandlers["{ImdbId}"] = m => movie.MovieMetadata.Value.ImdbId ?? string.Empty;
            tokenHandlers["{TmdbId}"] = m => movie.MovieMetadata.Value.TmdbId.ToString();
        }

        private void AddMovieFileTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, MovieFile movieFile)
        {
            tokenHandlers["{Original Title}"] = m => GetOriginalTitle(movieFile);
            tokenHandlers["{Original Filename}"] = m => GetOriginalFileName(movieFile);

            tokenHandlers["{Release Group}"] = m => movieFile.ReleaseGroup ?? m.DefaultValue("Radarr");
        }

        private void AddQualityTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie, MovieFile movieFile)
        {
            if (movieFile?.Quality?.Quality == null)
            {
                tokenHandlers["{Quality Full}"] = m => "";
                tokenHandlers["{Quality Title}"] = m => "";
                tokenHandlers["{Quality Proper}"] = m => "";
                tokenHandlers["{Quality Real}"] = m => "";
                return;
            }

            var qualityTitle = _qualityDefinitionService.Get(movieFile.Quality.Quality).Title;
            var qualityProper = GetQualityProper(movie, movieFile.Quality);
            var qualityReal = GetQualityReal(movie, movieFile.Quality);

            tokenHandlers["{Quality Full}"] = m => string.Format("{0} {1} {2}", qualityTitle, qualityProper, qualityReal);
            tokenHandlers["{Quality Title}"] = m => qualityTitle;
            tokenHandlers["{Quality Proper}"] = m => qualityProper;
            tokenHandlers["{Quality Real}"] = m => qualityReal;
        }

        private static readonly IReadOnlyDictionary<string, int> MinimumMediaInfoSchemaRevisions =
            new Dictionary<string, int>(FileNameBuilderTokenEqualityComparer.Instance)
        {
            { MediaInfoVideoDynamicRangeToken, 5 },
            { MediaInfoVideoDynamicRangeTypeToken, 10 }
        };

        private void AddMediaInfoTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, MovieFile movieFile)
        {
            if (movieFile.MediaInfo == null)
            {
                _logger.Trace("Media info is unavailable for {0}", movieFile);

                return;
            }

            var sceneName = movieFile.GetSceneOrFileName();

            var videoCodec = MediaInfoFormatter.FormatVideoCodec(movieFile.MediaInfo, sceneName);
            var audioCodec = MediaInfoFormatter.FormatAudioCodec(movieFile.MediaInfo, sceneName);
            var audioChannels = MediaInfoFormatter.FormatAudioChannels(movieFile.MediaInfo);
            var audioLanguages = movieFile.MediaInfo.AudioLanguages ?? new List<string>();
            var subtitles = movieFile.MediaInfo.Subtitles ?? new List<string>();

            var mediaInfoAudioLanguages = GetLanguagesToken(audioLanguages);
            if (!mediaInfoAudioLanguages.IsNullOrWhiteSpace())
            {
                mediaInfoAudioLanguages = $"[{mediaInfoAudioLanguages}]";
            }

            var mediaInfoAudioLanguagesAll = mediaInfoAudioLanguages;
            if (mediaInfoAudioLanguages == "[EN]")
            {
                mediaInfoAudioLanguages = string.Empty;
            }

            var mediaInfoSubtitleLanguages = GetLanguagesToken(subtitles);
            if (!mediaInfoSubtitleLanguages.IsNullOrWhiteSpace())
            {
                mediaInfoSubtitleLanguages = $"[{mediaInfoSubtitleLanguages}]";
            }

            var videoBitDepth = movieFile.MediaInfo.VideoBitDepth > 0 ? movieFile.MediaInfo.VideoBitDepth.ToString() : 8.ToString();
            var audioChannelsFormatted = audioChannels > 0 ?
                                audioChannels.ToString("F1", CultureInfo.InvariantCulture) :
                                string.Empty;

            var mediaInfo3D = movieFile.MediaInfo.VideoMultiViewCount > 1 ? "3D" : string.Empty;

            tokenHandlers["{MediaInfo Video}"] = m => videoCodec;
            tokenHandlers["{MediaInfo VideoCodec}"] = m => videoCodec;
            tokenHandlers["{MediaInfo VideoBitDepth}"] = m => videoBitDepth;

            tokenHandlers["{MediaInfo Audio}"] = m => audioCodec;
            tokenHandlers["{MediaInfo AudioCodec}"] = m => audioCodec;
            tokenHandlers["{MediaInfo AudioChannels}"] = m => audioChannelsFormatted;
            tokenHandlers["{MediaInfo AudioLanguages}"] = m => mediaInfoAudioLanguages;
            tokenHandlers["{MediaInfo AudioLanguagesAll}"] = m => mediaInfoAudioLanguagesAll;

            tokenHandlers["{MediaInfo SubtitleLanguages}"] = m => mediaInfoSubtitleLanguages;
            tokenHandlers["{MediaInfo SubtitleLanguagesAll}"] = m => mediaInfoSubtitleLanguages;

            tokenHandlers["{MediaInfo 3D}"] = m => mediaInfo3D;

            tokenHandlers["{MediaInfo Simple}"] = m => $"{videoCodec} {audioCodec}";
            tokenHandlers["{MediaInfo Full}"] = m => $"{videoCodec} {audioCodec}{mediaInfoAudioLanguages} {mediaInfoSubtitleLanguages}";

            tokenHandlers[MediaInfoVideoDynamicRangeToken] =
                m => MediaInfoFormatter.FormatVideoDynamicRange(movieFile.MediaInfo);
            tokenHandlers[MediaInfoVideoDynamicRangeTypeToken] =
                m => MediaInfoFormatter.FormatVideoDynamicRangeType(movieFile.MediaInfo);
        }

        private void AddCustomFormats(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie, MovieFile movieFile, List<CustomFormat> customFormats = null)
        {
            if (customFormats == null)
            {
                movieFile.Movie = movie;
                customFormats = CustomFormatCalculationService.ParseCustomFormat(movieFile, _formatService.All());
            }

            tokenHandlers["{Custom Formats}"] = m => string.Join(" ", customFormats.Where(x => x.IncludeCustomFormatWhenRenaming));
        }

        private string GetLanguagesToken(List<string> mediaInfoLanguages)
        {
            var tokens = new List<string>();
            foreach (var item in mediaInfoLanguages)
            {
                if (!string.IsNullOrWhiteSpace(item) && item != "und")
                {
                    tokens.Add(item.Trim());
                }
            }

            for (int i = 0; i < tokens.Count; i++)
            {
                try
                {
                    var token = tokens[i].ToLowerInvariant();
                    if (Iso639BTMap.TryGetValue(token, out var mapped))
                    {
                        token = mapped;
                    }

                    var cultureInfo = new CultureInfo(token);
                    tokens[i] = cultureInfo.TwoLetterISOLanguageName.ToUpper();
                }
                catch
                {
                }
            }

            return string.Join("+", tokens.Distinct());
        }

        private void UpdateMediaInfoIfNeeded(string pattern, MovieFile movieFile, Movie movie)
        {
            if (movie.Path.IsNullOrWhiteSpace())
            {
                return;
            }

            var schemaRevision = movieFile.MediaInfo != null ? movieFile.MediaInfo.SchemaRevision : 0;
            var matches = TitleRegex.Matches(pattern);

            var shouldUpdateMediaInfo = matches.Cast<Match>()
                .Select(m => MinimumMediaInfoSchemaRevisions.GetValueOrDefault(m.Value, -1))
                .Any(r => schemaRevision < r);

            if (shouldUpdateMediaInfo)
            {
                _mediaInfoUpdater.Update(movieFile, movie);
            }
        }

        private string ReplaceTokens(string pattern, Dictionary<string, Func<TokenMatch, string>> tokenHandlers, NamingConfig namingConfig)
        {
            return TitleRegex.Replace(pattern, match => ReplaceToken(match, tokenHandlers, namingConfig));
        }

        private string ReplaceToken(Match match, Dictionary<string, Func<TokenMatch, string>> tokenHandlers, NamingConfig namingConfig)
        {
            var tokenMatch = new TokenMatch
            {
                RegexMatch = match,
                Prefix = match.Groups["prefix"].Value,
                Separator = match.Groups["separator"].Value,
                Suffix = match.Groups["suffix"].Value,
                Token = match.Groups["token"].Value,
                CustomFormat = match.Groups["customFormat"].Value
            };

            if (tokenMatch.CustomFormat.IsNullOrWhiteSpace())
            {
                tokenMatch.CustomFormat = null;
            }

            var tokenHandler = tokenHandlers.GetValueOrDefault(tokenMatch.Token, m => string.Empty);

            var replacementText = tokenHandler(tokenMatch).Trim();

            if (tokenMatch.Token.All(t => !char.IsLetter(t) || char.IsLower(t)))
            {
                replacementText = replacementText.ToLower();
            }
            else if (tokenMatch.Token.All(t => !char.IsLetter(t) || char.IsUpper(t)))
            {
                replacementText = replacementText.ToUpper();
            }

            if (!tokenMatch.Separator.IsNullOrWhiteSpace())
            {
                replacementText = replacementText.Replace(" ", tokenMatch.Separator);
            }

            replacementText = CleanFileName(replacementText, namingConfig.ReplaceIllegalCharacters, namingConfig.ColonReplacementFormat);

            if (!replacementText.IsNullOrWhiteSpace())
            {
                replacementText = tokenMatch.Prefix + replacementText + tokenMatch.Suffix;
            }

            return replacementText;
        }

        private string ReplaceNumberToken(string token, int value)
        {
            var split = token.Trim('{', '}').Split(':');
            if (split.Length == 1)
            {
                return value.ToString("0");
            }

            return value.ToString(split[1]);
        }

        private string GetQualityProper(Movie movie, QualityModel quality)
        {
            if (quality.Revision.Version > 1)
            {
                return "Proper";
            }

            return string.Empty;
        }

        private string GetQualityReal(Movie movie, QualityModel quality)
        {
            if (quality.Revision.Real > 0)
            {
                return "REAL";
            }

            return string.Empty;
        }

        private string GetOriginalTitle(MovieFile movieFile)
        {
            if (movieFile.SceneName.IsNullOrWhiteSpace())
            {
                return GetOriginalFileName(movieFile);
            }

            return movieFile.SceneName;
        }

        private string GetOriginalFileName(MovieFile movieFile)
        {
            if (movieFile.RelativePath.IsNullOrWhiteSpace())
            {
                return Path.GetFileNameWithoutExtension(movieFile.Path);
            }

            return Path.GetFileNameWithoutExtension(movieFile.RelativePath);
        }

        private string ReplaceReservedDeviceNames(string input)
        {
            // Replace reserved windows device names with an alternative
            return ReservedDeviceNamesRegex.Replace(input, match => match.Value.Replace(".", "_"));
        }
    }

    internal sealed class TokenMatch
    {
        public Match RegexMatch { get; set; }
        public string Prefix { get; set; }
        public string Separator { get; set; }
        public string Suffix { get; set; }
        public string Token { get; set; }
        public string CustomFormat { get; set; }

        public string DefaultValue(string defaultValue)
        {
            if (string.IsNullOrEmpty(Prefix) && string.IsNullOrEmpty(Suffix))
            {
                return defaultValue;
            }
            else
            {
                return string.Empty;
            }
        }
    }

    public enum MultiEpisodeStyle
    {
        Extend = 0,
        Duplicate = 1,
        Repeat = 2,
        Scene = 3,
        Range = 4,
        PrefixedRange = 5
    }
}
