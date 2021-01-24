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
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Profiles.Releases;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Organizer
{
    public interface IBuildFileNames
    {
        string BuildBookFileName(Author author, Edition edition, BookFile bookFile, NamingConfig namingConfig = null, List<string> preferredWords = null);
        string BuildBookFilePath(Author author, Edition edition, string fileName, string extension);
        string BuildBookPath(Author author);
        BasicNamingConfig GetBasicNamingConfig(NamingConfig nameSpec);
        string GetAuthorFolder(Author author, NamingConfig namingConfig = null);
    }

    public class FileNameBuilder : IBuildFileNames
    {
        private readonly INamingConfigService _namingConfigService;
        private readonly IQualityDefinitionService _qualityDefinitionService;
        private readonly IPreferredWordService _preferredWordService;
        private readonly ICached<BookFormat[]> _trackFormatCache;
        private readonly Logger _logger;

        private static readonly Regex TitleRegex = new Regex(@"\{(?<prefix>[- ._\[(]*)(?<token>(?:[a-z0-9]+)(?:(?<separator>[- ._]+)(?:[a-z0-9]+))?)(?::(?<customFormat>[a-z0-9]+))?(?<suffix>[- ._)\]]*)\}",
                                                             RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex TrackRegex = new Regex(@"(?<track>\{track(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex MediumRegex = new Regex(@"(?<medium>\{medium(?:\:0+)?})",
                                                               RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex SeasonEpisodePatternRegex = new Regex(@"(?<separator>(?<=})[- ._]+?)?(?<seasonEpisode>s?{season(?:\:0+)?}(?<episodeSeparator>[- ._]?[ex])(?<episode>{episode(?:\:0+)?}))(?<separator>[- ._]+?(?={))?",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex ReleaseDateRegex = new Regex(@"\{Release(\s|\W|_)Year\}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex AuthorNameRegex = new Regex(@"(?<token>\{(?:Author)(?<separator>[- ._])(Clean)?Name(The)?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static readonly Regex BookTitleRegex = new Regex(@"(?<token>\{(?:Book)(?<separator>[- ._])(Clean)?Title(The)?\})",
                                                                            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex FileNameCleanupRegex = new Regex(@"([- ._])(\1)+", RegexOptions.Compiled);
        private static readonly Regex TrimSeparatorsRegex = new Regex(@"[- ._]$", RegexOptions.Compiled);

        private static readonly Regex ScenifyRemoveChars = new Regex(@"(?<=\s)(,|<|>|\/|\\|;|:|'|""|\||`|~|!|\?|@|$|%|^|\*|-|_|=){1}(?=\s)|('|:|\?|,)(?=(?:(?:s|m)\s)|\s|$)|(\(|\)|\[|\]|\{|\})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ScenifyReplaceChars = new Regex(@"[\/]", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        //TODO: Support Written numbers (One, Two, etc) and Roman Numerals (I, II, III etc)
        private static readonly Regex MultiPartCleanupRegex = new Regex(@"(?:\(\d+\)|(Part|Pt\.?)\s?\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly char[] TrackTitleTrimCharacters = new[] { ' ', '.', '?' };

        private static readonly Regex TitlePrefixRegex = new Regex(@"^(The|An|A) (.*?)((?: *\([^)]+\))*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public FileNameBuilder(INamingConfigService namingConfigService,
                               IQualityDefinitionService qualityDefinitionService,
                               ICacheManager cacheManager,
                               IPreferredWordService preferredWordService,
                               Logger logger)
        {
            _namingConfigService = namingConfigService;
            _qualityDefinitionService = qualityDefinitionService;
            _preferredWordService = preferredWordService;
            _trackFormatCache = cacheManager.GetCache<BookFormat[]>(GetType(), "bookFormat");
            _logger = logger;
        }

        public string BuildBookFileName(Author author, Edition edition, BookFile bookFile, NamingConfig namingConfig = null, List<string> preferredWords = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            if (!namingConfig.RenameBooks)
            {
                return GetOriginalFileName(bookFile);
            }

            if (namingConfig.StandardBookFormat.IsNullOrWhiteSpace())
            {
                throw new NamingFormatException("File name format cannot be empty");
            }

            var pattern = namingConfig.StandardBookFormat;

            var subFolders = pattern.Split(new[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
            var safePattern = subFolders.Aggregate("", (current, folderLevel) => Path.Combine(current, folderLevel));

            var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);

            AddAuthorTokens(tokenHandlers, author);
            AddBookTokens(tokenHandlers, edition);
            AddBookFileTokens(tokenHandlers, bookFile);
            AddQualityTokens(tokenHandlers, author, bookFile);
            AddMediaInfoTokens(tokenHandlers, bookFile);
            AddPreferredWords(tokenHandlers, author, bookFile, preferredWords);

            var fileName = ReplaceTokens(safePattern, tokenHandlers, namingConfig).Trim();
            fileName = FileNameCleanupRegex.Replace(fileName, match => match.Captures[0].Value[0].ToString());
            fileName = TrimSeparatorsRegex.Replace(fileName, string.Empty);

            return fileName;
        }

        public string BuildBookFilePath(Author author, Edition edition, string fileName, string extension)
        {
            Ensure.That(extension, () => extension).IsNotNullOrWhiteSpace();

            var path = BuildBookPath(author);

            return Path.Combine(path, fileName + extension);
        }

        public string BuildBookPath(Author author)
        {
            return author.Path;
        }

        public BasicNamingConfig GetBasicNamingConfig(NamingConfig nameSpec)
        {
            var trackFormat = GetTrackFormat(nameSpec.StandardBookFormat).LastOrDefault();

            if (trackFormat == null)
            {
                return new BasicNamingConfig();
            }

            var basicNamingConfig = new BasicNamingConfig
            {
                Separator = trackFormat.Separator
            };

            var titleTokens = TitleRegex.Matches(nameSpec.StandardBookFormat);

            foreach (Match match in titleTokens)
            {
                var separator = match.Groups["separator"].Value;
                var token = match.Groups["token"].Value;

                if (!separator.Equals(" "))
                {
                    basicNamingConfig.ReplaceSpaces = true;
                }

                if (token.StartsWith("{Author", StringComparison.InvariantCultureIgnoreCase))
                {
                    basicNamingConfig.IncludeAuthorName = true;
                }

                if (token.StartsWith("{Book", StringComparison.InvariantCultureIgnoreCase))
                {
                    basicNamingConfig.IncludeBookTitle = true;
                }

                if (token.StartsWith("{Quality", StringComparison.InvariantCultureIgnoreCase))
                {
                    basicNamingConfig.IncludeQuality = true;
                }
            }

            return basicNamingConfig;
        }

        public string GetAuthorFolder(Author author, NamingConfig namingConfig = null)
        {
            if (namingConfig == null)
            {
                namingConfig = _namingConfigService.GetConfig();
            }

            var tokenHandlers = new Dictionary<string, Func<TokenMatch, string>>(FileNameBuilderTokenEqualityComparer.Instance);

            AddAuthorTokens(tokenHandlers, author);

            return CleanFolderName(ReplaceTokens(namingConfig.AuthorFolderFormat, tokenHandlers, namingConfig));
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

        public static string CleanFileName(string name, bool replace = true)
        {
            string result = name;
            string[] badCharacters = { "\\", "/", "<", ">", "?", "*", ":", "|", "\"" };
            string[] goodCharacters = { "+", "+", "", "", "!", "-", "-", "", "" };

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

        private void AddAuthorTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Author author)
        {
            tokenHandlers["{Author Name}"] = m => author.Name;
            tokenHandlers["{Author CleanName}"] = m => CleanTitle(author.Name);
            tokenHandlers["{Author NameThe}"] = m => TitleThe(author.Name);

            if (author.Metadata.Value.Disambiguation != null)
            {
                tokenHandlers["{Author Disambiguation}"] = m => author.Metadata.Value.Disambiguation;
            }
        }

        private void AddBookTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Edition edition)
        {
            tokenHandlers["{Book Title}"] = m => edition.Title;
            tokenHandlers["{Book CleanTitle}"] = m => CleanTitle(edition.Title);
            tokenHandlers["{Book TitleThe}"] = m => TitleThe(edition.Title);

            if (edition.Disambiguation != null)
            {
                tokenHandlers["{Book Disambiguation}"] = m => edition.Disambiguation;
            }

            if (edition.ReleaseDate.HasValue)
            {
                tokenHandlers["{Release Year}"] = m => edition.ReleaseDate.Value.Year.ToString();
            }
            else
            {
                tokenHandlers["{Release Year}"] = m => "Unknown";
            }
        }

        private void AddBookFileTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, BookFile bookFile)
        {
            tokenHandlers["{Original Title}"] = m => GetOriginalTitle(bookFile);
            tokenHandlers["{Original Filename}"] = m => GetOriginalFileName(bookFile);
            tokenHandlers["{Release Group}"] = m => bookFile.ReleaseGroup ?? m.DefaultValue("Readarr");
        }

        private void AddQualityTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Author author, BookFile bookFile)
        {
            var qualityTitle = _qualityDefinitionService.Get(bookFile.Quality.Quality).Title;
            var qualityProper = GetQualityProper(bookFile.Quality);

            //var qualityReal = GetQualityReal(author, bookFile.Quality);
            tokenHandlers["{Quality Full}"] = m => string.Format("{0}", qualityTitle);
            tokenHandlers["{Quality Title}"] = m => qualityTitle;
            tokenHandlers["{Quality Proper}"] = m => qualityProper;

            //tokenHandlers["{Quality Real}"] = m => qualityReal;
        }

        private void AddMediaInfoTokens(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, BookFile bookFile)
        {
            if (bookFile.MediaInfo == null)
            {
                _logger.Trace("Media info is unavailable for {0}", bookFile);

                return;
            }

            var audioCodec = MediaInfoFormatter.FormatAudioCodec(bookFile.MediaInfo);
            var audioChannels = MediaInfoFormatter.FormatAudioChannels(bookFile.MediaInfo);
            var audioChannelsFormatted = audioChannels > 0 ?
                                audioChannels.ToString("F1", CultureInfo.InvariantCulture) :
                                string.Empty;

            tokenHandlers["{MediaInfo AudioCodec}"] = m => audioCodec;
            tokenHandlers["{MediaInfo AudioChannels}"] = m => audioChannelsFormatted;
            tokenHandlers["{MediaInfo AudioBitRate}"] = m => MediaInfoFormatter.FormatAudioBitrate(bookFile.MediaInfo);
            tokenHandlers["{MediaInfo AudioBitsPerSample}"] = m => MediaInfoFormatter.FormatAudioBitsPerSample(bookFile.MediaInfo);
            tokenHandlers["{MediaInfo AudioSampleRate}"] = m => MediaInfoFormatter.FormatAudioSampleRate(bookFile.MediaInfo);
        }

        private void AddPreferredWords(Dictionary<string, Func<TokenMatch, string>> tokenHandlers, Author author, BookFile bookFile, List<string> preferredWords = null)
        {
            if (preferredWords == null)
            {
                preferredWords = _preferredWordService.GetMatchingPreferredWords(author, bookFile.GetSceneOrFileName());
            }

            tokenHandlers["{Preferred Words}"] = m => string.Join(" ", preferredWords);
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

            replacementText = CleanFileName(replacementText, namingConfig.ReplaceIllegalCharacters);

            if (!replacementText.IsNullOrWhiteSpace())
            {
                replacementText = tokenMatch.Prefix + replacementText + tokenMatch.Suffix;
            }

            return replacementText;
        }

        private BookFormat[] GetTrackFormat(string pattern)
        {
            return _trackFormatCache.Get(pattern, () => SeasonEpisodePatternRegex.Matches(pattern).OfType<Match>()
                .Select(match => new BookFormat
                {
                    BookSeparator = match.Groups["episodeSeparator"].Value,
                    Separator = match.Groups["separator"].Value,
                    BookPattern = match.Groups["episode"].Value,
                }).ToArray());
        }

        private string GetQualityProper(QualityModel quality)
        {
            if (quality.Revision.Version > 1)
            {
                if (quality.Revision.IsRepack)
                {
                    return "Repack";
                }

                return "Proper";
            }

            return string.Empty;
        }

        private string GetOriginalTitle(BookFile bookFile)
        {
            if (bookFile.SceneName.IsNullOrWhiteSpace())
            {
                return GetOriginalFileName(bookFile);
            }

            return bookFile.SceneName;
        }

        private string GetOriginalFileName(BookFile bookFile)
        {
            return Path.GetFileNameWithoutExtension(bookFile.Path);
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
