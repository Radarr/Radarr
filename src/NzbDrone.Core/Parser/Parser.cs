using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Movies;
using TinyIoC;

namespace NzbDrone.Core.Parser
{
    public static class Parser
    {
        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(Parser));

        private static readonly Regex[] ReportMovieTitleRegex = new[]
        {
            //Special, Despecialized, etc. Edition Movies, e.g: Mission.Impossible.3.Special.Edition.2011
            new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*\(?(?<edition>(((Extended.|Ultimate.)?(Director.?s|Collector.?s|Theatrical|Ultimate|Final(?=(.(Cut|Edition|Version)))|Extended|Rogue|Special|Despecialized|\d{2,3}(th)?.Anniversary)(.(Cut|Edition|Version))?(.(Extended|Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit))?|((Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit|Edition|Restored|((2|3|4)in1))))))\)?.{1,3}(?<year>(19|20)\d{2}(?!p|i|\d+|\]|\W\d+)))+(\W+|_|$)(?!\\)",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),

            //Special, Despecialized, etc. Edition Movies, e.g: Mission.Impossible.3.2011.Special.Edition //TODO: Seems to slow down parsing heavily!
            /*new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(19|20)\d{2}(?!p|i|(19|20)\d{2}|\]|\W(19|20)\d{2})))+(\W+|_|$)(?!\\)\(?(?<edition>(((Extended.|Ultimate.)?(Director.?s|Collector.?s|Theatrical|Ultimate|Final(?=(.(Cut|Edition|Version)))|Extended|Rogue|Special|Despecialized|\d{2,3}(th)?.Anniversary)(.(Cut|Edition|Version))?(.(Extended|Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit))?|((Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit|Edition|Restored|((2|3|4)in1))))))\)?",
                          RegexOptions.IgnoreCase | RegexOptions.Compiled),*/

            //Normal movie format, e.g: Mission.Impossible.3.2011
            new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(19|20)\d{2}(?!p|i|(19|20)\d{2}|\]|\W(19|20)\d{2})))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            //PassThePopcorn Torrent names: Star.Wars[PassThePopcorn]
            new Regex(@"^(?<title>.+?)?(?:(?:[-_\W](?<![()\[!]))*(?<year>(\[\w *\])))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

            //That did not work? Maybe some tool uses [] for years. Who would do that?
            new Regex(@"^(?<title>(?![(\[]).+?)?(?:(?:[-_\W](?<![)!]))*(?<year>(19|20)\d{2}(?!p|i|\d+|\W\d+)))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

			//As a last resort for movies that have ( or [ in their title.
			new Regex(@"^(?<title>.+?)?(?:(?:[-_\W](?<![)\[!]))*(?<year>(19|20)\d{2}(?!p|i|\d+|\]|\W\d+)))+(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),

        };

        private static readonly Regex[] ReportMovieTitleFolderRegex = new[]
        {
            //When year comes first.
            new Regex(@"^(?:(?:[-_\W](?<![)!]))*(?<year>(19|20)\d{2}(?!p|i|\d+|\W\d+)))+(\W+|_|$)(?<title>.+?)?$")
        };

        private static readonly Regex[] ReportMovieTitleLenientRegexBefore = new[]
        {
            //Some german or french tracker formats
            new Regex(@"^(?<title>(?![(\[]).+?)((\W|_))(?:(?<!(19|20)\d{2}.)(German|French|TrueFrench))(.+?)(?=((19|20)\d{2}|$))(?<year>(19|20)\d{2}(?!p|i|\d+|\]|\W\d+))?(\W+|_|$)(?!\\)", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        };

        private static readonly Regex[] ReportMovieTitleLenientRegexAfter = new Regex[]
        {

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

                //b00bs - Started appearing January 2015
                new Regex(@"^b00bs$", RegexOptions.Compiled | RegexOptions.IgnoreCase)
            };

        //Regex to detect whether the title was reversed.
        private static readonly Regex ReversedTitleRegex = new Regex(@"[-._ ](p027|p0801|\d{2}E\d{2}S)[-._ ]", RegexOptions.Compiled);

        private static readonly Regex NormalizeRegex = new Regex(@"((?:\b|_)(?<!^|\W\w\W)(a(?!$|\W\w\W)|an|the|and|or|of)(?:\b|_))|\W|_",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex FileExtensionRegex = new Regex(@"\.[a-z0-9]{2,4}$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReportImdbId = new Regex(@"(?<imdbid>tt\d{7})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleTitleRegex = new Regex(@"\s*(?:480[ip]|576[ip]|720[ip]|1080[ip]|2160[ip]|[xh][\W_]?26[45]|DD\W?5\W1|[<>?*:|]|848x480|1280x720|1920x1080|(8|10)b(it)?)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SimpleReleaseTitleRegex = new Regex(@"\s*(?:[<>?*:|])", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex WebsitePrefixRegex = new Regex(@"^\[\s*[a-z]+(\.[a-z]+)+\s*\][- ]*",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AirDateRegex = new Regex(@"^(.*?)(?<!\d)((?<airyear>\d{4})[_.-](?<airmonth>[0-1][0-9])[_.-](?<airday>[0-3][0-9])|(?<airmonth>[0-1][0-9])[_.-](?<airday>[0-3][0-9])[_.-](?<airyear>\d{4}))(?!\d)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex SixDigitAirDateRegex = new Regex(@"(?<=[_.-])(?<airdate>(?<!\d)(?<airyear>[1-9]\d{1})(?<airmonth>[0-1][0-9])(?<airday>[0-3][0-9]))(?=[_.-])",
                                                                        RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CleanReleaseGroupRegex = new Regex(@"^(.*?[-._ ](S\d+E\d+)[-._ ])|(-(RP|1|NZBGeek|Obfuscated|sample))+$",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex CleanTorrentSuffixRegex = new Regex(@"\[(?:ettv|rartv|rarbg|cttv)\]$",
                                                                   RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex ReleaseGroupRegex = new Regex(@"-(?<releasegroup>[a-z0-9]+)(?<!WEB-DL|480p|720p|1080p|2160p)(?:\b|[-._ ])",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex AnimeReleaseGroupRegex = new Regex(@"^(?:\[(?<subgroup>(?!\s).+?(?<!\s))\](?:_|-|\s|\.)?)",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex YearInTitleRegex = new Regex(@"^(?<title>.+?)(?:\W|_)?(?<year>\d{4})",
                                                                RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex WordDelimiterRegex = new Regex(@"(\s|\.|,|_|-|=|'|\|)+", RegexOptions.Compiled);
        private static readonly Regex SpecialCharRegex = new Regex(@"(\&|\:|\\|\/)+", RegexOptions.Compiled);
        private static readonly Regex PunctuationRegex = new Regex(@"[^\w\s]", RegexOptions.Compiled);
        private static readonly Regex CommonWordRegex = new Regex(@"\b(a|an|the|and|or|of)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex SpecialEpisodeWordRegex = new Regex(@"\b(part|special|edition|christmas)\b\s?", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex DuplicateSpacesRegex = new Regex(@"\s{2,}", RegexOptions.Compiled);

        private static readonly Regex RequestInfoRegex = new Regex(@"\[.+?\]", RegexOptions.Compiled);

        private static readonly Regex ReportYearRegex = new Regex(@"^.*(?<year>(19|20)\d{2}).*$", RegexOptions.Compiled);

        private static readonly Regex ReportEditionRegex = new Regex(@"(?<edition>(((Extended.|Ultimate.)?(Director.?s|Collector.?s|Theatrical|Ultimate|Final(?=(.(Cut|Edition|Version)))|Extended|Rogue|Special|Despecialized|\d{2,3}(th)?.Anniversary)(.(Cut|Edition|Version))?(.(Extended|Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit))?|((Uncensored|Remastered|Unrated|Uncut|IMAX|Fan.?Edit|Edition|Restored|((2|3|4)in1))))))\)?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly string[] Numbers = new[] { "zero", "one", "two", "three", "four", "five", "six", "seven", "eight", "nine" };
               private static Dictionary<String, String> _umlautMappings = new Dictionary<string, string>
        {
            {"ö", "oe"},
            {"ä", "ae"},
            {"ü", "ue"},
        };

        private static ParsedMovieInfo ParseMoviePath(string path, bool isLenient)
        {
            var fileInfo = new FileInfo(path);

            var result = ParseMovieTitle(fileInfo.Name, isLenient, true);

            if (result == null)
            {
                Logger.Debug("Attempting to parse movie info using directory and file names. {0}", fileInfo.Directory.Name);
                result = ParseMovieTitle(fileInfo.Directory.Name + " " + fileInfo.Name, isLenient);
            }

            if (result == null)
            {
                Logger.Debug("Attempting to parse movie info using directory name. {0}", fileInfo.Directory.Name);
                result = ParseMovieTitle(fileInfo.Directory.Name + fileInfo.Extension, isLenient);
            }

            return result;

        }

        public static ParsedMovieInfo ParseMovieTitle(string title, bool isLenient, bool isDir = false)
        {

            ParsedMovieInfo realResult = null;
            try
            {
                if (!ValidateBeforeParsing(title)) return null;

                Logger.Debug("Parsing string '{0}'", title);

                if (ReversedTitleRegex.IsMatch(title))
                {
                    var titleWithoutExtension = RemoveFileExtension(title).ToCharArray();
                    Array.Reverse(titleWithoutExtension);

                    title = new string(titleWithoutExtension) + title.Substring(titleWithoutExtension.Length);

                    Logger.Debug("Reversed name detected. Converted to '{0}'", title);
                }

                var simpleTitle = SimpleTitleRegex.Replace(title, string.Empty);

                simpleTitle = RemoveFileExtension(simpleTitle);

                var simpleReleaseTitle = SimpleReleaseTitleRegex.Replace(title, string.Empty);
                simpleReleaseTitle = RemoveFileExtension(simpleReleaseTitle);

                // TODO: Quick fix stripping [url] - prefixes.
                simpleTitle = WebsitePrefixRegex.Replace(simpleTitle, string.Empty);

                simpleTitle = CleanTorrentSuffixRegex.Replace(simpleTitle, string.Empty);

                var allRegexes = ReportMovieTitleRegex.ToList();

                if (isDir)
                {
                    allRegexes.AddRange(ReportMovieTitleFolderRegex);
                }

                if (isLenient)
                {
                    allRegexes.InsertRange(0, ReportMovieTitleLenientRegexBefore);

                    allRegexes.AddRange(ReportMovieTitleLenientRegexAfter);
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
                                if (result.MovieTitle.IsNotNullOrWhiteSpace())
                                {
                                    simpleReleaseTitle = simpleReleaseTitle.Replace(result.MovieTitle, result.MovieTitle.Contains(".") ? "A.Movie" : "A Movie");
                                }

                                result.SimpleReleaseTitle = simpleReleaseTitle;

                                realResult = result;

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
                    Logger.Error(e, "An error has occurred while trying to parse " + title);
            }

            Logger.Debug("Unable to parse {0}", title);
            return realResult;
        }

        public static ParsedMovieInfo ParseMinimalMovieTitle(string title, string foundTitle, int foundYear)
        {
            var result = new ParsedMovieInfo {MovieTitle = foundTitle};

            var languageTitle = Regex.Replace(title.Replace(".", " "), foundTitle, "A Movie", RegexOptions.IgnoreCase);

            result.Languages = LanguageParser.ParseLanguages(title);
            Logger.Debug("Language parsed: {0}", result.Languages.ToExtendedString());

            result.Quality = QualityParser.ParseQuality(title);
            Logger.Debug("Quality parsed: {0}", result.Quality);

            if (result.Edition.IsNullOrWhiteSpace())
            {
                result.Edition = ParseEdition(languageTitle);
            }

            result.ReleaseGroup = ParseReleaseGroup(title);

            result.ImdbId = ParseImdbId(title);

            Logger.Debug("Release Group parsed: {0}", result.ReleaseGroup);

            if (foundYear > 1800)
            {
                result.Year = foundYear;
            }
            else
            {
                var match = ReportYearRegex.Match(title);
                if (match.Success && match.Groups["year"].Value != null)
                {
                    int year = 1290;
                    if (int.TryParse(match.Groups["year"].Value, out year))
                    {
                        result.Year = year;
                    }
                    else
                    {
                        result.Year = year;
                    }
                }
            }

            return result;
        }

        public static string ParseImdbId(string title)
        {
            var match = ReportImdbId.Match(title);
            if (match.Success)
            {
                if (match.Groups["imdbid"].Value != null)
                {
                    if (match.Groups["imdbid"].Length == 9)
                    {
                        return match.Groups["imdbid"].Value;
                    }
                }
            }

            return "";
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
            var bytes = Encoding.GetEncoding("ISO-8859-8").GetBytes(value);
            value = Encoding.ASCII.GetString(bytes);

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

        public static string CleanSeriesTitle(this string title)
        {
            long number = 0;

            //If Title only contains numbers return it as is.
            if (long.TryParse(title, out number))
                return title;

            return ReplaceGermanUmlauts(NormalizeRegex.Replace(title, string.Empty).ToLower()).RemoveAccent();
        }

        public static string NormalizeEpisodeTitle(string title)
        {
            title = SpecialEpisodeWordRegex.Replace(title, string.Empty);
            title = PunctuationRegex.Replace(title, " ");
            title = DuplicateSpacesRegex.Replace(title, " ");

            return title.Trim()
                        .ToLower();
        }

        public static string NormalizeTitle(string title)
        {
            title = WordDelimiterRegex.Replace(title, " ");
            title = PunctuationRegex.Replace(title, string.Empty);
            title = CommonWordRegex.Replace(title, string.Empty);
            title = DuplicateSpacesRegex.Replace(title, " ");
            title = SpecialCharRegex.Replace(title, string.Empty);

            return title.Trim().ToLower();
        }

        public static string ParseReleaseGroup(string title)
        {
            title = title.Trim();
            title = RemoveFileExtension(title);
            title = WebsitePrefixRegex.Replace(title, "");

            var animeMatch = AnimeReleaseGroupRegex.Match(title);

            if (animeMatch.Success)
            {
                return animeMatch.Groups["subgroup"].Value;
            }

            title = CleanReleaseGroupRegex.Replace(title, "");

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

        private static SeriesTitleInfo GetSeriesTitleInfo(string title)
        {
            var seriesTitleInfo = new SeriesTitleInfo();
            seriesTitleInfo.Title = title;

            var match = YearInTitleRegex.Match(title);

            if (!match.Success)
            {
                seriesTitleInfo.TitleWithoutYear = title;
            }

            else
            {
                seriesTitleInfo.TitleWithoutYear = match.Groups["title"].Value;
                seriesTitleInfo.Year = Convert.ToInt32(match.Groups["year"].Value);
            }

            return seriesTitleInfo;
        }

        private static ParsedMovieInfo ParseMovieMatchCollection(MatchCollection matchCollection)
        {
            if (!matchCollection[0].Groups["title"].Success || matchCollection[0].Groups["title"].Value == "(")
            {
                return null;
            }


            var movieName = matchCollection[0].Groups["title"].Value./*Replace('.', ' ').*/Replace('_', ' ');
            movieName = RequestInfoRegex.Replace(movieName, "").Trim(' ');

			var parts = movieName.Split('.');
			movieName = "";
			int n = 0;
			bool previousAcronym = false;
			string nextPart = "";
			foreach (var part in parts)
			{
				if (parts.Length >= n+2)
				{
					nextPart = parts[n+1];
				}
				if (part.Length == 1 && part.ToLower() != "a" && !int.TryParse(part, out n))
				{
					movieName += part + ".";
					previousAcronym = true;
				}
				else if (part.ToLower() == "a" && (previousAcronym == true || nextPart.Length == 1))
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

            int airYear;
            int.TryParse(matchCollection[0].Groups["year"].Value, out airYear);

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
