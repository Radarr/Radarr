using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;

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
        private readonly INamingConfigService _namingConfigService;
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly ICached<EpisodeFormat[]> _episodeFormatCache;
        private readonly ICached<AbsoluteEpisodeFormat[]> _absoluteEpisodeFormatCache;
        private readonly Logger _logger;

        private static readonly Regex TitleRegex = new Regex(@"\{(?<prefix>[- ._\[(]*)(?<token>(?:[a-z0-9]+)(?:(?<separator>[- ._]+)(?:[a-z0-9]+))?)(?::(?<customFormat>[a-z0-9]+))?(?<suffix>[- ._)\]]*)\}",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex EpisodeRegex = new Regex(@"(?<episode>\{episode(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex TagsRegex = new Regex(@"(?<tags>\{tags(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex SeasonRegex = new Regex(@"(?<season>\{season(?:\:0+)?})",
                                                              RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex AbsoluteEpisodeRegex = new Regex(@"(?<absolute>\{absolute(?:\:0+)?})",
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
                               ICacheManager cacheManager,
                               Logger logger)
        {
            _namingConfigService = namingConfigService;
            _qualityDefinitionService = qualityDefinitionService;
            //_movieFormatCache = cacheManager.GetCache<MovieFormat>(GetType(), "movieFormat");
            _episodeFormatCache = cacheManager.GetCache<EpisodeFormat[]>(GetType(), "episodeFormat");
            _absoluteEpisodeFormatCache = cacheManager.GetCache<AbsoluteEpisodeFormat[]>(GetType(), "absoluteEpisodeFormat");
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

            AddMovieTokens(tokenHandlers, movie);
            AddReleaseDateTokens(tokenHandlers, movie.Year);
            AddImdbIdTokens(tokenHandlers, movie.ImdbId);
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
            AddImdbIdTokens(tokenHandlers, movie.ImdbId);

            if(movie.MovieFile != null)
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

            //var episodeFormat = GetEpisodeFormat(nameSpec.StandardMovieFormat).LastOrDefault();

            //if (episodeFormat == null)
            //{
            //    return new BasicNamingConfig();
            //}

            //var basicNamingConfig = new BasicNamingConfig
            //{
            //    Separator = episodeFormat.Separator,
            //    NumberStyle = episodeFormat.SeasonEpisodePattern
            //};

            //var titleTokens = TitleRegex.Matches(nameSpec.StandardMovieFormat);

            //foreach (Match match in titleTokens)
            //{
            //    var separator = match.Groups["separator"].Value;
            //    var token = match.Groups["token"].Value;

            //    if (!separator.Equals(" "))
            //    {
            //        basicNamingConfig.ReplaceSpaces = true;
            //    }

            //    if (token.StartsWith("{Series", StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        basicNamingConfig.IncludeSeriesTitle = true;
            //    }

            //    if (token.StartsWith("{Episode", StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        basicNamingConfig.IncludeEpisodeTitle = true;
            //    }

            //    if (token.StartsWith("{Quality", StringComparison.InvariantCultureIgnoreCase))
            //    {
            //        basicNamingConfig.IncludeQuality = true;
            //    }
            //}

            //return basicNamingConfig;
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
            AddImdbIdTokens(tokenHandlers, movie.ImdbId);

            if (movie.MovieFile != null)
            {
                AddQualityTokens(tokenHandlers, movie, movieFile);
                AddMediaInfoTokens(tokenHandlers, movieFile);
                AddMovieFileTokens(tokenHandlers, movieFile);
                AddTagsTokens(tokenHandlers, movieFile);
            }
            else
            {
                AddMovieFileTokens(tokenHandlers, new MovieFile { SceneName = $"{movie.Title} {movie.Year}", RelativePath = $"{movie.Title} {movie.Year}"});
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

        private void AddImdbIdTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, string imdbId)
        {
            tokenHandlers["{IMDb Id}"] = m => $"{imdbId}";
        }

        private void AddMovieFileTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, MovieFile episodeFile)
        {
            tokenHandlers["{Original Title}"] = m => GetOriginalTitle(episodeFile);
            tokenHandlers["{Original Filename}"] = m => GetOriginalFileName(episodeFile);
            //tokenHandlers["{IMDb Id}"] = m =>
            tokenHandlers["{Release Group}"] = m => episodeFile.ReleaseGroup ?? m.DefaultValue("Radarr");
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

            tokenHandlers["{Quality Full}"] = m => String.Format("{0} {1} {2}", qualityTitle, qualityProper, qualityReal);
            tokenHandlers["{Quality Title}"] = m => qualityTitle;
            tokenHandlers["{Quality Proper}"] = m => qualityProper;
            tokenHandlers["{Quality Real}"] = m => qualityReal;
        }

        private void AddMediaInfoTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, MovieFile movieFile)
        {
            if (movieFile.MediaInfo == null) return;

            string videoCodec;
            switch (movieFile.MediaInfo.VideoCodec)
            {
                case "AVC":
                    if (movieFile.SceneName.IsNotNullOrWhiteSpace() && Path.GetFileNameWithoutExtension(movieFile.SceneName).Contains("h264"))
                    {
                        videoCodec = "h264";
                    }
                    else
                    {
                        videoCodec = "x264";
                    }
                    break;

                case "V_MPEGH/ISO/HEVC":
                    if (movieFile.SceneName.IsNotNullOrWhiteSpace() && Path.GetFileNameWithoutExtension(movieFile.SceneName).Contains("h265"))
                    {
                        videoCodec = "h265";
                    }
                    else
                    {
                        videoCodec = "x265";
                    }
                    break;

                case "MPEG-2 Video":
                    videoCodec = "MPEG2";
                    break;

                default:
                    videoCodec = movieFile.MediaInfo.VideoCodec;
                    break;
            }

            string audioCodec;
            switch (movieFile.MediaInfo.AudioFormat)
            {
                case "AC-3":
                    audioCodec = "AC3";
                    break;

                case "E-AC-3":
                    audioCodec = "EAC3";
                    break;

                case "Atmos / TrueHD":
                    audioCodec = "Atmos TrueHD";
                    break;

                case "MPEG Audio":
                    if (movieFile.MediaInfo.AudioProfile == "Layer 3")
                    {
                        audioCodec = "MP3";
                    }
                    else
                    {
                        audioCodec = movieFile.MediaInfo.AudioFormat;
                    }
                    break;

                case "DTS":
                    if (movieFile.MediaInfo.AudioProfile == "ES" || movieFile.MediaInfo.AudioProfile == "ES Discrete" || movieFile.MediaInfo.AudioProfile == "ES Matrix")
                    {
                        audioCodec = "DTS-ES";
                    }
                    else if (movieFile.MediaInfo.AudioProfile == "MA")
                    {
                        audioCodec = "DTS-HD MA";
                    }
                    else if (movieFile.MediaInfo.AudioProfile == "HRA")
                    {
                        audioCodec = "DTS-HD HRA";
                    }
                    else if (movieFile.MediaInfo.AudioProfile == "X")
                    {
                        audioCodec = "DTS-X";
                    }
                    else
                    {
                        audioCodec = movieFile.MediaInfo.AudioFormat;
                    }
                    break;

                default:
                    audioCodec = movieFile.MediaInfo.AudioFormat;
                    break;
            }

            var mediaInfoAudioLanguages = GetLanguagesToken(movieFile.MediaInfo.AudioLanguages);
            if (!mediaInfoAudioLanguages.IsNullOrWhiteSpace())
            {
                mediaInfoAudioLanguages = string.Format("[{0}]", mediaInfoAudioLanguages);
            }
            var mediaInfoAudioLanguagesAll = mediaInfoAudioLanguages;
            if (mediaInfoAudioLanguages == "[EN]")
            {
                mediaInfoAudioLanguages = string.Empty;
            }


            var mediaInfoSubtitleLanguages = GetLanguagesToken(movieFile.MediaInfo.Subtitles);
            if (!mediaInfoSubtitleLanguages.IsNullOrWhiteSpace())
            {
                mediaInfoSubtitleLanguages = string.Format("[{0}]", mediaInfoSubtitleLanguages);
            }

            var videoBitDepth = movieFile.MediaInfo.VideoBitDepth > 0 ? movieFile.MediaInfo.VideoBitDepth.ToString() : string.Empty;
            var audioChannels = movieFile.MediaInfo.FormattedAudioChannels > 0 ?
                                movieFile.MediaInfo.FormattedAudioChannels.ToString("F1", CultureInfo.InvariantCulture) :
                                string.Empty;

            tokenHandlers["{MediaInfo Video}"] = m => videoCodec;
            tokenHandlers["{MediaInfo VideoCodec}"] = m => videoCodec;
            tokenHandlers["{MediaInfo VideoBitDepth}"] = m => videoBitDepth;

            tokenHandlers["{MediaInfo Audio}"] = m => audioCodec;
            tokenHandlers["{MediaInfo AudioCodec}"] = m => audioCodec;
            tokenHandlers["{MediaInfo AudioChannels}"] = m => audioChannels;

            tokenHandlers["{MediaInfo Simple}"] = m => string.Format("{0} {1}", videoCodec, audioCodec);

            tokenHandlers["{MediaInfo Full}"] = m => string.Format("{0} {1}{2} {3}", videoCodec, audioCodec, mediaInfoAudioLanguages, mediaInfoSubtitleLanguages);
            tokenHandlers["{MediaInfo AudioLanguages}"] = m => mediaInfoAudioLanguages;
            tokenHandlers["{MediaInfo AudioLanguagesAll}"] = m => mediaInfoAudioLanguagesAll;
            tokenHandlers["{MediaInfo SubtitleLanguages}"] = m => mediaInfoSubtitleLanguages;
        }

        private string GetLanguagesToken(string mediaInfoLanguages)
        {
            List<string> tokens = new List<string>();
            foreach (var item in mediaInfoLanguages.Split('/'))
            {
                if (!string.IsNullOrWhiteSpace(item))
                    tokens.Add(item.Trim());
            }

            var cultures = System.Globalization.CultureInfo.GetCultures(System.Globalization.CultureTypes.NeutralCultures);
            for (int i = 0; i < tokens.Count; i++)
            {
                try
                {
                    var cultureInfo = cultures.FirstOrDefault(p => p.EnglishName == tokens[i]);

                    if (cultureInfo != null)
                        tokens[i] = cultureInfo.TwoLetterISOLanguageName.ToUpper();
                }
                catch
                {
                }
            }

            return string.Join("+", tokens.Distinct());
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
            if (split.Length == 1) return value.ToString("0");

            return value.ToString(split[1]);
        }

        private EpisodeFormat[] GetEpisodeFormat(string pattern)
        {
            return _episodeFormatCache.Get(pattern, () => SeasonEpisodePatternRegex.Matches(pattern).OfType<Match>()
                .Select(match => new EpisodeFormat
                {
                    EpisodeSeparator = match.Groups["episodeSeparator"].Value,
                    Separator = match.Groups["separator"].Value,
                    EpisodePattern = match.Groups["episode"].Value,
                    SeasonEpisodePattern = match.Groups["seasonEpisode"].Value,
                }).ToArray());
        }

        private string GetQualityProper(Movie movie, QualityModel quality)
        {
            if (quality.Revision.Version > 1)
            {
                return "Proper";
            }

            return String.Empty;
        }

        private string GetQualityReal(Movie movie, QualityModel quality)
        {
            if (quality.Revision.Real > 0)
            {
                return "REAL";
            }

            return string.Empty;
        }

        private string GetOriginalTitle(MovieFile episodeFile)
        {
            if (episodeFile.SceneName.IsNullOrWhiteSpace())
            {
                return GetOriginalFileName(episodeFile);
            }

            return episodeFile.SceneName;
        }

        private string GetOriginalFileName(MovieFile episodeFile)
        {
            if (episodeFile.RelativePath.IsNullOrWhiteSpace())
            {
                return Path.GetFileNameWithoutExtension(episodeFile.Path);
            }

            return Path.GetFileNameWithoutExtension(episodeFile.RelativePath);
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
