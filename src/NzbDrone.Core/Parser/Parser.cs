using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Parser
{
    public static class Parser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(Parser));

        private static readonly Regex EditionRegex = new Regex(@"\(?\b(?<edition>(((Recut.|Extended.|Ultimate.)?(Director.?s|Collector.?s|Theatrical|Ultimate|Extended|Despecialized|(Special|Rouge|Final|Assembly)(?=(.(Cut|Edition|Version)))|\d{2,3}(th)?.Anniversary)(.(Cut|Edition|Version))?(.(Extended|Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit))?|((Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit|Edition|Restored|((2|3|4)in1))))))\b\)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex ReportEditionRegex = new Regex(@"^.+?" + EditionRegex, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex[] ReportMovieTitleRegex = new[]
        {
            //Some german or french tracker formats (missing year, ...) (Only applies to german and French/TrueFrench releases) - see ParserFixture for examples and tests
            new Regex(@"^(?<title>(?![(\[]).+?)((\W|_))(" + EditionRegex + @".{1,3})?(?:(?<!(19|20)\d{2}.*?)(German|French|TrueFrench))(.+?)(?=((19|20)\d{2}|$))(?<year>(19|20)\d{2}(?!p|i|\d+|\]|\W\d+))?(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            //Special, Despecialized, etc. Edition Movies, e.g: Mission.Impossible.3.Special.Edition.2011
            new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*" + EditionRegex + @".{1,3}(?<year>(1(8|9)|20)\d{2}(?!p|i|\d+|\]|\W\d+)))+(\W+|_|$)(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

            //Special, Despecialized, etc. Edition Movies, e.g: Mission.Impossible.3.2011.Special.Edition //TODO: Seems to slow down parsing heavily!
            /*new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(19|20)\d{2}(?!p|i|(19|20)\d{2}|\]|\W(19|20)\d{2})))+(\W+|_|$)(?!\\)\(?(?<edition>(((Extended.|Ultimate.)?(Director.?s|Collector.?s|Theatrical|Ultimate|Final(?=(.(Cut|Edition|Version)))|Extended|Rogue|Special|Despecialized|\d{2,3}(th)?.Anniversary)(.(Cut|Edition|Version))?(.(Extended|Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit))?|((Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit|Edition|Restored|((2|3|4)in1))))))\)?",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),*/

            //Normal movie format, e.g: Mission.Impossible.3.2011
            new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(1(8|9)|20)\d{2}(?!p|i|(1(8|9)|20)\d{2}|\]|\W(1(8|9)|20)\d{2})))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            //PassThePopcorn Torrent names: Star.Wars[PassThePopcorn]
            new Regex(@"^(?<title>.+?)?(?:(?:[-_\W](?<![()\[!]))*(?<year>(\[\w *\])))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            //That did not work? Maybe some tool uses [] for years. Who would do that?
            new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)!]))*(?<year>(1(8|9)|20)\d{2}(?!p|i|\d+|\W\d+)))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            //As a last resort for movies that have ( or [ in their title.
            new Regex(@"^(?<title>.+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(1(8|9)|20)\d{2}(?!p|i|\d+|\]|\W\d+)))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        };

        private static readonly Regex[] ReportMovieTitleFolderRegex = new[]
        {
            //When year comes first.
            new Regex(@"^(?:(?:[-_\W](?<![)!]))*(?<year>(19|20)\d{2}(?!p|i|\d+|\W\d+)))+(\W+|_|$)(?<title>.+?)?$")
        };

        private static readonly Regex[] RejectHashedReleasesRegex = new Regex[]
            {
                // Generic match for md5 and mixed-case hashes.
                new Regex(@"^[0-9a-zA-Z]{32}", RegexOptions.Compiled),

                // Generic match for shorter lower-case hashes.
                new Regex(@"^[a-z0-9]{24}$", RegexOptions.Compiled),

                // Format seen on some NZBGeek releases
                // Be very strict with these coz they are very close to the valid 101 ep numbering.
                new Regex(@"^[A-Z]{11}\d{3}$", RegexOptions.Compiled),
                new Regex(@"^[a-z]{12}\d{3}$", RegexOptions.Compiled),

                //Backup filename (Unknown origins)
                new Regex(@"^Backup_\d{5,}S\d{2}-\d{2}$", RegexOptions.Compiled),

                //123 - Started appearing December 2014
                new Regex(@"^123$", RegexOptions.Compiled),

                //abc - Started appearing January 2015
                new Regex(@"^abc$", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                //abc - Started appearing 2020
                new Regex(@"^abc[-_. ]xyz", RegexOptions.Compiled | RegexOptions.IgnoreCase),

                //b00bs - Started appearing January 2015
                new Regex(@"^b00bs$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

        //Regex to detect whether the title was reversed.
        private static readonly Regex ReversedTitleRegex = new Regex(@"(?:^|[-._ ])(p027|p0801)[-._ ]", RegexOptions.Compiled);

        private static readonly Regex NormalizeRegex = new Regex(@"((?:\b|_)(?<!^|[^a-zA-Z0-9_']\w[^a-zA-Z0-9_'])(a(?!$|[^a-zA-Z0-9_']\w[^a-zA-Z0-9_'])|an|the|and|or|of)(?:\b|_))|\W|_",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FileExtensionRegex = new Regex(@"\.[a-z0-9]{2,4}$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReportImdbId = new Regex(@"(?<imdbid>tt\d{7,8})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex ReportTmdbId = new Regex(@"tmdb(id)?-(?<tmdbid>\d+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace SimpleTitleRegex = new RegexReplace(@"\s*(?:480[ip]|576[ip]|720[ip]|1080[ip]|2160[ip]|[xh][\W_]?26[45]|DD\W?5\W1|[<>?*:|]|848x480|1280x720|1920x1080|(8|10)b(it)?)",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleReleaseTitleRegex = new Regex(@"\s*(?:[<>?*:|])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly RegexReplace WebsitePrefixRegex = new RegexReplace(@"^\[\s*[-a-z]+(\.[a-z]+)+\s*\][- ]*|^www\.[a-z]+\.(?:com|net|org)[ -]*",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace WebsitePostfixRegex = new RegexReplace(@"\[\s*[-a-z]+(\.[a-z0-9]+)+\s*\]$",
                                                        string.Empty,
                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace CleanReleaseGroupRegex = new RegexReplace(@"(-(RP|1|NZBGeek|Obfuscated|Obfuscation|Scrambled|sample|Pre|postbot|xpost|Rakuv[a-z0-9]*|WhiteRev|BUYMORE|AsRequested|AlternativeToRequested|GEROV|Z0iDS3N|Chamele0n|4P|4Planet|AlteZachen))+$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly RegexReplace CleanTorrentSuffixRegex = new RegexReplace(@"\[(?:ettv|rartv|rarbg|cttv)\]$",
                                                                string.Empty,
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CleanQualityBracketsRegex = new Regex(@"\[[a-z0-9 ._-]+\]$",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReleaseGroupRegex = new Regex(@"-(?<releasegroup>[a-z0-9]+(?<part2>-[a-z0-9]+)?(?!.+?(?:480p|720p|1080p|2160p)))(?<!(?:WEB-DL|Blu-Ray|480p|720p|1080p|2160p|DTS-HD|DTS-X|DTS-MA|DTS-ES|[ ._]\d{4}-\d{2}|-\d{2}|tmdb(id)?-(?<tmdbid>\d+)|(?<imdbid>tt\d{7,8}))(?:\k<part2>)?)(?:\b|[-._ ]|$)|[-._ ]\[(?<releasegroup>[a-z0-9]+)\]$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AnimeReleaseGroupRegex = new Regex(@"^(?:\[(?<subgroup>(?!\s).+?(?<!\s))\](?:_|-|\s|\.)?)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex YearInTitleRegex = new Regex(@"^(?<title>.+?)(?:\W|_)?(?<year>\d{4})",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        //Handle Exception Release Groups that don't follow -RlsGrp; Manual List
        //First Group is groups whose releases end with RlsGroup) or RlsGroup]  second group (entries after `(?=\]|\))|`) is name only...BE VERY CAREFUL WITH THIS, HIGH CHANCE OF FALSE POSITIVES
        private static readonly Regex ExceptionReleaseGroupRegex = new Regex(@"(?<releasegroup>(Tigole|Joy|YIFY|YTS.MX|FreetheFish|afm72|Anna|Bandi|Ghost|Kappa|MONOLITH|Qman|RZeroX|SAMPA|Silence|theincognito|t3nzin|Vyndros)(?=\]|\))|KRaLiMaRKo|E\.N\.D|D\-Z0N3)",
                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex WordDelimiterRegex = new Regex(@"(\s|\.|,|_|-|=|'|\|)+", RegexOptions.Compiled);
        private static readonly Regex SpecialCharRegex = new Regex(@"(\&|\:|\\|\/)+", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);
        private static readonly Regex CommonWordRegex = new Regex(@"\b(a|an|the|and|or|of)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SpecialEpisodeWordRegex = new Regex(@"\b(part|special|edition|christmas)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DuplicateSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        private static readonly Regex RequestInfoRegex = new Regex(@"^(?:\[.+?\])+", RegexOptions.Compiled);

        private static readonly string[] Numbers = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
        private static Dictionary<string, string> _umlautMappings = new Dictionary<string, string>
        {
            { "ö", "oe" },
            { "ä", "ae" },
            { "ü", "ue" },
        };

        public static ParsedMovieInfo ParseMoviePath(string path)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseMovieTitle(fileInfo.Name, true);

            if (result == null)
            {
                Logger.Debug("Attempting to parse movie info using directory and file names. {0}", fileInfo.Directory.Name);
                result = ParseMovieTitle(fileInfo.Directory.Name + " " + fileInfo.Name);
            }

            if (result == null)
            {
                Logger.Debug("Attempting to parse movie info using directory name. {0}", fileInfo.Directory.Name);
                result = ParseMovieTitle(fileInfo.Directory.Name + fileInfo.Extension);
            }

            return result;
        }

        public static ParsedMovieInfo ParseMovieTitle(string title, bool isDir = false)
        {
            var originalTitle = title;
            try
            {
                if (!ValidateBeforeParsing(title))
                {
                    return null;
                }

                Logger.Debug("Parsing string '{0}'", title);

                if (ReversedTitleRegex.IsMatch(title))
                {
                    var titleWithoutExtension = RemoveFileExtension(title).ToCharArray();
                    Array.Reverse(titleWithoutExtension);

                    title = new string(titleWithoutExtension) + title.Substring(titleWithoutExtension.Length);

                    Logger.Debug("Reversed name detected. Converted to '{0}'", title);
                }

                var releaseTitle = RemoveFileExtension(title);

                //Trim dashes from end
                releaseTitle = releaseTitle.Trim('-', '_');

                releaseTitle = releaseTitle.Replace("【", "[").Replace("】", "]");

                var simpleTitle = SimpleTitleRegex.Replace(releaseTitle);

                // TODO: Quick fix stripping [url] - prefixes.
                simpleTitle = WebsitePrefixRegex.Replace(simpleTitle);
                simpleTitle = WebsitePostfixRegex.Replace(simpleTitle);

                simpleTitle = CleanTorrentSuffixRegex.Replace(simpleTitle);

                simpleTitle = CleanQualityBracketsRegex.Replace(simpleTitle, m =>
                {
                    if (QualityParser.ParseQualityName(m.Value).Quality != Qualities.Quality.Unknown)
                    {
                        return string.Empty;
                    }

                    return m.Value;
                });

                var allRegexes = ReportMovieTitleRegex.ToList();

                if (isDir)
                {
                    allRegexes.AddRange(ReportMovieTitleFolderRegex);
                }

                foreach (var regex in allRegexes)
                {
                    var match = regex.Matches(simpleTitle);

                    if (match.Count != 0)
                    {
                        Logger.Trace(regex);
                        try
                        {
                            var result = ParseMovieMatchCollection(match);

                            if (result != null)
                            {
                                //TODO: Add tests for this!
                                var simpleReleaseTitle = SimpleReleaseTitleRegex.Replace(releaseTitle, string.Empty);

                                var simpleTitleReplaceString = match[0].Groups["title"].Success ? match[0].Groups["title"].Value : result.MovieTitle;

                                if (simpleTitleReplaceString.IsNotNullOrWhiteSpace())
                                {
                                    simpleReleaseTitle = simpleReleaseTitle.Replace(simpleTitleReplaceString, simpleTitleReplaceString.Contains(".") ? "A.Movie" : "A Movie");
                                }

                                result.ReleaseGroup = ParseReleaseGroup(simpleReleaseTitle);

                                var subGroup = GetSubGroup(match);
                                if (!subGroup.IsNullOrWhiteSpace())
                                {
                                    result.ReleaseGroup = subGroup;
                                }

                                Logger.Debug("Release Group parsed: {0}", result.ReleaseGroup);

                                result.Languages = LanguageParser.ParseLanguages(result.ReleaseGroup.IsNotNullOrWhiteSpace() ? simpleReleaseTitle.Replace(result.ReleaseGroup, "RlsGrp") : simpleReleaseTitle);
                                Logger.Debug("Languages parsed: {0}", string.Join(", ", result.Languages));

                                result.Quality = QualityParser.ParseQuality(title);
                                Logger.Debug("Quality parsed: {0}", result.Quality);

                                if (result.Edition.IsNullOrWhiteSpace())
                                {
                                    result.Edition = ParseEdition(simpleReleaseTitle);
                                    Logger.Debug("Edition parsed: {0}", result.Edition);
                                }

                                result.ReleaseHash = GetReleaseHash(match);
                                if (!result.ReleaseHash.IsNullOrWhiteSpace())
                                {
                                    Logger.Debug("Release Hash parsed: {0}", result.ReleaseHash);
                                }

                                result.OriginalTitle = originalTitle;
                                result.ReleaseTitle = releaseTitle;
                                result.SimpleReleaseTitle = simpleReleaseTitle;

                                result.ImdbId = ParseImdbId(simpleReleaseTitle);
                                result.TmdbId = ParseTmdbId(simpleReleaseTitle);

                                return result;
                            }
                        }
                        catch (InvalidDateException ex)
                        {
                            Logger.Debug(ex, ex.Message);
                            break;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                if (!title.ToLower().Contains("password") && !title.ToLower().Contains("yenc"))
                {
                    Logger.Error(e, "An error has occurred while trying to parse {0}", title);
                }
            }

            Logger.Debug("Unable to parse {0}", title);
            return null;
        }

        public static string ParseImdbId(string title)
        {
            var match = ReportImdbId.Match(title);
            if (match.Success)
            {
                if (match.Groups["imdbid"].Value != null)
                {
                    if (match.Groups["imdbid"].Length == 9 || match.Groups["imdbid"].Length == 10)
                    {
                        return match.Groups["imdbid"].Value;
                    }
                }
            }

            return "";
        }

        public static int ParseTmdbId(string title)
        {
            var match = ReportTmdbId.Match(title);
            if (match.Success)
            {
                if (match.Groups["tmdbid"].Value != null)
                {
                    return int.TryParse(match.Groups["tmdbid"].Value, out var tmdbId) ? tmdbId : 0;
                }
            }

            return 0;
        }

        public static string ParseEdition(string languageTitle)
        {
            var editionMatch = ReportEditionRegex.Match(languageTitle);

            if (editionMatch.Success && editionMatch.Groups["edition"].Value != null &&
                editionMatch.Groups["edition"].Value.IsNotNullOrWhiteSpace())
            {
                return editionMatch.Groups["edition"].Value.Replace(".", " ");
            }

            return "";
        }

        public static string ReplaceGermanUmlauts(string s)
        {
            var t = s;
            t = t.Replace("ä", "ae");
            t = t.Replace("ö", "oe");
            t = t.Replace("ü", "ue");
            t = t.Replace("Ä", "Ae");
            t = t.Replace("Ö", "Oe");
            t = t.Replace("Ü", "Ue");
            t = t.Replace("ß", "ss");
            return t;
        }

        public static string NormalizeImdbId(string imdbId)
        {
            var imdbRegex = new Regex(@"^(\d{1,10}|(tt)\d{1,10})$");

            if (!imdbRegex.IsMatch(imdbId))
            {
                return null;
            }

            if (imdbId.Length > 2)
            {
                imdbId = imdbId.Replace("tt", "").PadLeft(7, '0');
                return $"tt{imdbId}";
            }

            return null;
        }

        public static string ToUrlSlug(string value)
        {
            //First to lower case
            value = value.ToLowerInvariant();

            //Remove all accents
            value = value.RemoveAccent();

            //Replace spaces
            value = Regex.Replace(value, @"\s", "-", RegexOptions.Compiled);

            //Remove invalid chars
            value = Regex.Replace(value, @"[^a-z0-9\s-_]", "", RegexOptions.Compiled);

            //Trim dashes from end
            value = value.Trim('-', '_');

            //Replace double occurences of - or _
            value = Regex.Replace(value, @"([-_]){2,}", "$1", RegexOptions.Compiled);

            return value;
        }

        public static string CleanMovieTitle(this string title)
        {
            long number = 0;

            //If Title only contains numbers return it as is.
            if (long.TryParse(title, out number))
            {
                return title;
            }

            return ReplaceGermanUmlauts(NormalizeRegex.Replace(title, string.Empty).ToLower()).RemoveAccent();
        }

        public static string NormalizeEpisodeTitle(this string title)
        {
            title = SpecialEpisodeWordRegex.Replace(title, string.Empty);
            title = PunctuationRegex.Replace(title, " ");
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim()
                        .ToLower();
        }

        public static string NormalizeTitle(this string title)
        {
            title = WordDelimiterRegex.Replace(title, " ");
            title = PunctuationRegex.Replace(title, string.Empty);
            title = CommonWordRegex.Replace(title, string.Empty);
            title = DuplicateSpacesRegex.Replace(title, " ");
            title = SpecialCharRegex.Replace(title, string.Empty);

            return title.Trim().ToLower();
        }

        public static string SimplifyReleaseTitle(this string title)
        {
            return SimpleReleaseTitleRegex.Replace(title, string.Empty);
        }

        public static string ParseReleaseGroup(string title)
        {
            title = title.Trim();
            title = RemoveFileExtension(title);
            title = WebsitePrefixRegex.Replace(title);

            var animeMatch = AnimeReleaseGroupRegex.Match(title);

            if (animeMatch.Success)
            {
                return animeMatch.Groups["subgroup"].Value;
            }

            title = CleanReleaseGroupRegex.Replace(title);

            var exceptionMatch = ExceptionReleaseGroupRegex.Matches(title);

            if (exceptionMatch.Count != 0)
            {
                return exceptionMatch.OfType<Match>().Last().Groups["releasegroup"].Value;
            }

            var matches = ReleaseGroupRegex.Matches(title);

            if (matches.Count != 0)
            {
                var group = matches.OfType<Match>().Last().Groups["releasegroup"].Value;
                int groupIsNumeric;

                if (int.TryParse(group, out groupIsNumeric))
                {
                    return null;
                }

                return group;
            }

            return null;
        }

        public static string RemoveFileExtension(string title)
        {
            title = FileExtensionRegex.Replace(title, m =>
                {
                    var extension = m.Value.ToLower();
                    if (MediaFiles.MediaFileExtensions.Extensions.Contains(extension) || new[] { ".par2", ".nzb" }.Contains(extension))
                    {
                        return string.Empty;
                    }

                    return m.Value;
                });

            return title;
        }

        private static ParsedMovieInfo ParseMovieMatchCollection(MatchCollection matchCollection)
        {
            if (!matchCollection[0].Groups["title"].Success || matchCollection[0].Groups["title"].Value == "(")
            {
                return null;
            }

            var movieName = matchCollection[0].Groups["title"].Value.Replace('_', ' ');
            movieName = RequestInfoRegex.Replace(movieName, "").Trim(' ');

            var parts = movieName.Split('.');
            movieName = "";
            int n = 0;
            bool previousAcronym = false;
            string nextPart = "";
            foreach (var part in parts)
            {
                if (parts.Length >= n + 2)
                {
                    nextPart = parts[n + 1];
                }
                else
                {
                    nextPart = "";
                }

                if (part.Length == 1 && part.ToLower() != "a" && !int.TryParse(part, out _) &&
                    (previousAcronym || n < parts.Length - 1) &&
                    (previousAcronym || nextPart.Length != 1 || !int.TryParse(nextPart, out _)))
                {
                    movieName += part + ".";
                    previousAcronym = true;
                }
                else if (part.ToLower() == "a" && (previousAcronym || nextPart.Length == 1))
                {
                    movieName += part + ".";
                    previousAcronym = true;
                }
                else if (part.ToLower() == "dr")
                {
                    movieName += part + ".";
                    previousAcronym = true;
                }
                else
                {
                    if (previousAcronym)
                    {
                        movieName += " ";
                        previousAcronym = false;
                    }

                    movieName += part + " ";
                }

                n++;
            }

            movieName = movieName.Trim(' ');

            int.TryParse(matchCollection[0].Groups["year"].Value, out var airYear);

            ParsedMovieInfo result;

            result = new ParsedMovieInfo { Year = airYear };

            if (matchCollection[0].Groups["edition"].Success)
            {
                result.Edition = matchCollection[0].Groups["edition"].Value.Replace(".", " ");
            }

            result.MovieTitle = movieName;

            Logger.Debug("Movie Parsed. {0}", result);

            return result;
        }

        private static bool ValidateBeforeParsing(string title)
        {
            if (title.ToLower().Contains("password") && title.ToLower().Contains("yenc"))
            {
                Logger.Debug("");
                return false;
            }

            if (!title.Any(char.IsLetterOrDigit))
            {
                return false;
            }

            var titleWithoutExtension = RemoveFileExtension(title);

            if (RejectHashedReleasesRegex.Any(v => v.IsMatch(titleWithoutExtension)))
            {
                Logger.Debug("Rejected Hashed Release Title: " + title);
                return false;
            }

            return true;
        }

        private static string GetSubGroup(MatchCollection matchCollection)
        {
            var subGroup = matchCollection[0].Groups["subgroup"];

            if (subGroup.Success)
            {
                return subGroup.Value;
            }

            return string.Empty;
        }

        private static string GetReleaseHash(MatchCollection matchCollection)
        {
            var hash = matchCollection[0].Groups["hash"];

            if (hash.Success)
            {
                var hashValue = hash.Value.Trim('[', ']');

                if (hashValue.Equals("1280x720"))
                {
                    return string.Empty;
                }

                return hashValue;
            }

            return string.Empty;
        }
    }
}
