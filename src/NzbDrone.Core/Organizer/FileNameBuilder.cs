using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildFileName(Movie movie, MovieFile movieFile, NamingConfig namingConfig = null);
        string BuildFilePath(Movie movie, string fileName, string extension);
        string BuildMoviePath(Movie movie, NamingConfig namingConfig = null);
        BasicNamingConfig GetBasicNamingConfig(NamingConfig nameSpec);
        string GetMovieFolder(Movie movie, NamingConfig namingConfig = null);
    }

    public class FileNameBuilder : IBuildFileNames
    {
        private const string MediaInfoVideoDynamicRangeToken = "{MediaInfo VideoDynamicRange}";

        private readonly INamingConfigService _namingConfigService;
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly IUpdateMediaInfo _mediaInfoUpdater;
        private readonly Logger _logger;

        private static readonly Regex TitleRegex = new Regex(@"\{(?<prefix>[- ._\[(]*)(?<token>(?:[a-z0-9]+)(?:(?<separator>[- ._]+)(?:[a-z0-9]+))?)(?::(?<customFormat>[a-z0-9]+))?(?<suffix>[- ._)\]]*)\}",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex TagsRegex = new Regex(@"(?<tags>\{tags(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SeasonEpisodePatternRegex = new Regex(@"(?<separator>(?<=})[- ._]+?)?(?<seasonEpisode>s?{season(?:\:0+)?}(?<episodeSeparator>[- ._]?[ex])(?<episode>{episode(?:\:0+)?}))(?<separator>[- ._]+?(?={))?",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex AbsoluteEpisodePatternRegex = new Regex(@"(?<separator>(?<=})[- ._]+?)?(?<absolute>{absolute(?:\:0+)?})(?<separator>[- ._]+?(?={))?",
                                                                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex AirDateRegex = new Regex(@"\{Air(\s|\W|_)Date\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SeriesTitleRegex = new Regex(@"(?<token>\{(?:Series)(?<separator>[- ._])(Clean)?Title\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex MovieTitleRegex = new Regex(@"(?<token>\{((?:(Movie|Original))(?<separator>[- ._])(Clean)?(Title|Filename)(The)?)\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FileNameCleanupRegex = new Regex(@"([- ._])(\1)+", RegexOptions.Compiled);
        private static readonly Regex TrimSeparatorsRegex = new Regex(@"[- ._]$", RegexOptions.Compiled);

        private static readonly Regex ScenifyRemoveChars = new Regex(@"(?<=\s)(,|<|>|\/|\\|;|:|'|""|\||`|~|!|\?|@|$|%|^|\*|-|_|=){1}(?=\s)|('|:|\?|,)(?=(?:(?:s|m)\s)|\s|$)|(\(|\)|\[|\]|\{|\})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ScenifyReplaceChars = new Regex(@"[\/]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //TODO: Support Written numbers (One, Two, etc) and Roman Numerals (I, II, III etc)
        private static readonly Regex MultiPartCleanupRegex = new Regex(@"(?:\(\d+\)|(Part|Pt\.?)\s?\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly char[] EpisodeTitleTrimCharacters = new[] { ' ', '.', '?' };

        public FileNameBuilder(INamingConfigService namingConfigService,
                               IQualityDefinitionService qualityDefinitionService,
                               IUpdateMediaInfo mediaInfoUpdater,
                               Logger logger)
        {
            _namingConfigService = namingConfigService;
            _qualityDefinitionService = qualityDefinitionService;
            _mediaInfoUpdater = mediaInfoUpdater;
            _logger = logger;
        }

        public string BuildFileName(Movie movie, MovieFile movieFile, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            if (!namingConfig.RenameEpisodes)
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

            var fileName = ReplaceTokens(pattern, tokenHandlers, namingConfig).Trim();
            fileName = FileNameCleanupRegex.Replace(fileName, match => match.Captures[0].Value[0].ToString());
            fileName = TrimSeparatorsRegex.Replace(fileName, string.Empty);

            return fileName;
        }

        public string BuildFilePath(Movie movie, string fileName, string extension)
        {
            Ensure.That(extension, () => extension).IsNotNullOrWhiteSpace();

            var path = "";

            if (movie.PathState > 0)
            {
                path = movie.Path;
            }
            else
            {
                path = BuildMoviePath(movie);
            }

            return Path.Combine(path, fileName + extension);
        }

        public string BuildMoviePath(Movie movie, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            var path = movie.Path;
            var directory = new DirectoryInfo(path).Name;
            var parentDirectoryPath = new DirectoryInfo(path).Parent.FullName;

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

            var directoryName = ReplaceTokens(pattern, tokenHandlers, namingConfig).Trim();
            directoryName = FileNameCleanupRegex.Replace(directoryName, match => match.Captures[0].Value[0].ToString());
            directoryName = TrimSeparatorsRegex.Replace(directoryName, string.Empty);

            return Path.Combine(parentDirectoryPath, directoryName);
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

            string name = ReplaceTokens(namingConfig.MovieFolderFormat, tokenHandlers, namingConfig);
            return CleanFolderName(name, namingConfig.ReplaceIllegalCharacters, namingConfig.ColonReplacementFormat);
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
            string[] prefixes = { "The ", "An ", "A " };

            if (title.Length < 5)
            {
                return title;
            }

            foreach (string prefix in prefixes)
            {
                int prefix_length = prefix.Length;
                if (prefix.ToLower() == title.Substring(0, prefix_length).ToLower())
                {
                    title = title.Substring(prefix_length) + ", " + prefix.Trim();
                    break;
                }
            }

            return title.Trim();
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

        public static string CleanFolderName(string name, bool replace = true, ColonReplacementFormat colonReplacement = ColonReplacementFormat.Delete)
        {
            name = FileNameCleanupRegex.Replace(name, match => match.Captures[0].Value[0].ToString());
            name = name.Trim(' ', '.');

            return CleanFileName(name, replace, colonReplacement);
        }

        private void AddMovieTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie)
        {
            tokenHandlers["{Movie Title}"] = m => movie.Title;
            tokenHandlers["{Movie CleanTitle}"] = m => CleanTitle(movie.Title);
            tokenHandlers["{Movie Title The}"] = m => TitleThe(movie.Title);
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
            tokenHandlers["{Release Year}"] = m => string.Format("{0}", releaseYear.ToString()); //Do I need m.CustomFormat?
        }

        private void AddIdTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Movie movie)
        {
            tokenHandlers["{ImdbId}"] = m => movie.ImdbId ?? string.Empty;
            tokenHandlers["{TmdbId}"] = m => movie.TmdbId.ToString();
        }

        private void AddMovieFileTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, MovieFile movieFile)
        {
            tokenHandlers["{Original Title}"] = m => GetOriginalTitle(movieFile);
            tokenHandlers["{Original Filename}"] = m => GetOriginalFileName(movieFile);

            //tokenHandlers["{IMDb Id}"] = m =>
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
            { MediaInfoVideoDynamicRangeToken, 5 }
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
            var audioLanguages = movieFile.MediaInfo.AudioLanguages ?? string.Empty;
            var subtitles = movieFile.MediaInfo.Subtitles ?? string.Empty;

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

            var videoBitDepth = movieFile.MediaInfo.VideoBitDepth > 0 ? movieFile.MediaInfo.VideoBitDepth.ToString() : string.Empty;
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
        }

        private string GetLanguagesToken(string mediaInfoLanguages)
        {
            List<string> tokens = new List<string>();
            foreach (var item in mediaInfoLanguages.Split('/'))
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    tokens.Add(item.Trim());
                }
            }

            var cultures = CultureInfo.GetCultures(CultureTypes.NeutralCultures);
            for (int i = 0; i < tokens.Count; i++)
            {
                try
                {
                    var cultureInfo = cultures.FirstOrDefault(p => p.EnglishName == tokens[i]);

                    if (cultureInfo != null)
                    {
                        tokens[i] = cultureInfo.TwoLetterISOLanguageName.ToUpper();
                    }
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
