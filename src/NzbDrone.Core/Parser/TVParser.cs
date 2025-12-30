using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.TV;

namespace NzbDrone.Core.Parser
{
    public static class TVParser
    {
        private const RegexOptions StandardOptions = RegexOptions.IgnoreCase | RegexOptions.Compiled;

        private static readonly Logger Logger = NzbDroneLogger.GetLogger(typeof(TVParser));
        private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(1);

        private static readonly Regex[] EpisodeRegex = new[]
        {
            // Standard S01E01 format with optional multi-episode (S01E01E02 or S01E01-E02)
            new Regex(@"(?:^|[^\w])s(?<season>\d{1,2})(?:[.-])?e(?<episode>\d{1,3})(?:(?:[-e])+(?<episode2>\d{1,3}))*(?:\W|$)", StandardOptions, RegexTimeout),

            // S01 (season only, no episode)
            new Regex(@"(?:^|[^\w])s(?<season>\d{1,2})(?:[^\w]|$)(?!e\d)", StandardOptions, RegexTimeout),

            // Season 01 Episode 01 / Season 1 Episode 1
            new Regex(@"(?:Season|Series)\W*(?<season>\d{1,2})\W*(?:Episode|Ep)\W*(?<episode>\d{1,3})(?:(?:[-e])+(?<episode2>\d{1,3}))*", StandardOptions, RegexTimeout),

            // Season only (Season 01, Season 1)
            new Regex(@"(?:Season|Series)\W*(?<season>\d{1,2})(?:[^\w]|$)", StandardOptions, RegexTimeout),

            // 1x01 format
            new Regex(@"(?:^|[^\w])(?<season>\d{1,2})x(?<episode>\d{1,3})(?:(?:[-x])+(?<episode2>\d{1,3}))*(?:\W|$)", StandardOptions, RegexTimeout),

            // Part format - Part 1, Part I, Part.1
            new Regex(@"(?:^|[^\w])(?:Part|Pt)[.\s-]*(?<episode>\d{1,3}|[IVXLC]+)(?:\W|$)", StandardOptions, RegexTimeout),
        };

        private static readonly Regex[] DailyEpisodeRegex = new[]
        {
            // 2024.01.15 or 2024-01-15
            new Regex(@"(?:^|[^\d])(?<airyear>\d{4})[.-](?<airmonth>\d{2})[.-](?<airday>\d{2})(?:[^\d]|$)", StandardOptions, RegexTimeout),

            // 15.01.2024 or 15-01-2024
            new Regex(@"(?:^|[^\d])(?<airday>\d{2})[.-](?<airmonth>\d{2})[.-](?<airyear>\d{4})(?:[^\d]|$)", StandardOptions, RegexTimeout),
        };

        private static readonly Regex[] AnimeEpisodeRegex = new[]
        {
            // [SubGroup] Title - 01v2 or [SubGroup] Title - 01 (version detection)
            new Regex(@"^\[(?<subgroup>[^\]]+)\][\s._-]*(?<title>.+?)[\s._-]+(?<episode>\d{1,4})(?:v(?<version>\d{1,2}))?(?:[\s._-]*\[|\(|$)", StandardOptions, RegexTimeout),

            // [SubGroup] Title - 01-12 (batch range)
            new Regex(@"^\[(?<subgroup>[^\]]+)\][\s._-]*(?<title>.+?)[\s._-]+(?<episode>\d{1,4})[\s._-]*-[\s._-]*(?<episode2>\d{1,4})(?:[\s._-]*\[|\(|$)", StandardOptions, RegexTimeout),

            // Show Title - S01E01 - Episode Title [Subgroup] (hybrid format)
            new Regex(@"^(?<title>.+?)[\s._-]+S(?<season>\d{1,2})E(?<episode>\d{1,3})[\s._-]+.+?\[(?<subgroup>[^\]]+)\]", StandardOptions, RegexTimeout),

            // Show - 01 format (no brackets, absolute numbering)
            new Regex(@"^(?<title>[^\[\]]+?)[\s._-]+(?<episode>\d{2,4})(?:v(?<version>\d{1,2}))?(?:[\s._-]+|$)", StandardOptions, RegexTimeout),
        };

        private static readonly Regex SeasonPackRegex = new Regex(
            @"(?:^|[^\w])(?:Complete|COMPLETE|Full|Season|S)[\s._-]*(?<season>\d{1,2})[\s._-]*(?:Complete|COMPLETE)?(?:[^\w]|$)",
            StandardOptions,
            RegexTimeout);

        private static readonly Regex SpecialEpisodeRegex = new Regex(
            @"(?:^|[^\w])(?:SP|Special|OVA|OAD|ONA|OAV|Pilot)[\s._-]*(?<episode>\d{1,3})?(?:[^\w]|$)",
            StandardOptions,
            RegexTimeout);

        private static readonly Dictionary<string, StreamingSource> StreamingSourceMap = new Dictionary<string, StreamingSource>(StringComparer.OrdinalIgnoreCase)
        {
            { "AMZN", StreamingSource.Amazon },
            { "Amazon", StreamingSource.Amazon },
            { "NF", StreamingSource.Netflix },
            { "Netflix", StreamingSource.Netflix },
            { "DSNP", StreamingSource.Disney },
            { "DisneyPlus", StreamingSource.Disney },
            { "Disney+", StreamingSource.Disney },
            { "ATVP", StreamingSource.AppleTV },
            { "AppleTV", StreamingSource.AppleTV },
            { "HULU", StreamingSource.Hulu },
            { "HBO", StreamingSource.HBO },
            { "HMAX", StreamingSource.HBO },
            { "HBOMax", StreamingSource.HBO },
            { "MAX", StreamingSource.HBO },
            { "PCOK", StreamingSource.Peacock },
            { "Peacock", StreamingSource.Peacock },
            { "PMTP", StreamingSource.Paramount },
            { "ParamountPlus", StreamingSource.Paramount },
            { "CR", StreamingSource.CrunchyRoll },
            { "CrunchyRoll", StreamingSource.CrunchyRoll },
            { "FUNI", StreamingSource.Funimation },
            { "Funimation", StreamingSource.Funimation },
            { "HIDV", StreamingSource.Hidive },
            { "Hidive", StreamingSource.Hidive },
            { "VRV", StreamingSource.VRV },
            { "RKTN", StreamingSource.Rakuten },
            { "iTunes", StreamingSource.ITunes },
            { "VUDU", StreamingSource.Vudu },
            { "STAN", StreamingSource.Stan },
            { "iP", StreamingSource.BBC },
            { "BBC", StreamingSource.BBC },
            { "ITV", StreamingSource.ITV },
            { "4OD", StreamingSource.All4 },
            { "NOW", StreamingSource.Now },
            { "CANAL", StreamingSource.Canal },
            { "WAKA", StreamingSource.Wakanim },
            { "Wakanim", StreamingSource.Wakanim },
            { "DCU", StreamingSource.DCUniverse },
            { "QIBI", StreamingSource.Quibi },
            { "SPEC", StreamingSource.Spectrum },
            { "SHO", StreamingSource.Showtime },
            { "Showtime", StreamingSource.Showtime },
            { "STRP", StreamingSource.Starz },
            { "Starz", StreamingSource.Starz },
            { "BRTBX", StreamingSource.BritBox }
        };

        public static ParsedEpisodeInfo ParseEpisodeTitle(string title, bool isSpecial = false)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var simpleTitle = ParserCommon.SimplifyTitle(title);
            var normalizedTitle = title.Replace('_', ' ').Replace('.', ' ').Trim();

            Logger.Debug("Parsing episode: {0}", title);

            var result = new ParsedEpisodeInfo
            {
                OriginalTitle = title,
                ReleaseTitle = title,
                SimpleReleaseTitle = simpleTitle
            };

            // Try anime format first (most specific)
            if (TryParseAnime(normalizedTitle, result))
            {
                result.IsAbsoluteNumbering = true;
                ParseAdditionalInfo(title, result);
                return result;
            }

            // Try daily format
            if (TryParseDaily(normalizedTitle, result))
            {
                result.IsDaily = true;
                ParseAdditionalInfo(title, result);
                return result;
            }

            // Try standard format
            if (TryParseStandard(normalizedTitle, result))
            {
                ParseAdditionalInfo(title, result);
                return result;
            }

            // Check for special episodes
            if (isSpecial || TryParseSpecial(normalizedTitle, result))
            {
                result.IsPossibleSpecialEpisode = true;
                result.SeasonNumber = 0;
                ParseAdditionalInfo(title, result);
                return result;
            }

            Logger.Debug("Unable to parse episode info from: {0}", title);
            return null;
        }

#pragma warning disable S3776 // Cognitive complexity is acceptable for parser methods
        private static bool TryParseStandard(string title, ParsedEpisodeInfo result)
        {
#pragma warning restore S3776
            foreach (var regex in EpisodeRegex)
            {
                var match = regex.Match(title);
                if (match.Success)
                {
                    var seasonGroup = match.Groups["season"];
                    var episodeGroup = match.Groups["episode"];
                    var episode2Group = match.Groups["episode2"];

                    if (seasonGroup.Success)
                    {
                        result.SeasonNumber = int.Parse(seasonGroup.Value);
                    }

                    if (episodeGroup.Success)
                    {
                        var episodes = new List<int> { int.Parse(episodeGroup.Value) };

                        if (episode2Group.Success && episode2Group.Captures.Count > 0)
                        {
                            foreach (Capture capture in episode2Group.Captures)
                            {
                                episodes.Add(int.Parse(capture.Value));
                            }
                        }

                        result.EpisodeNumbers = episodes.Distinct().OrderBy(e => e).ToArray();
                    }
                    else if (seasonGroup.Success)
                    {
                        result.FullSeason = true;
                        result.EpisodeNumbers = Array.Empty<int>();
                    }

                    result.SeriesTitle = ExtractSeriesTitle(title, match);
                    return true;
                }
            }

            // Check for season packs
            var seasonPackMatch = SeasonPackRegex.Match(title);
            if (seasonPackMatch.Success)
            {
                result.SeasonNumber = int.Parse(seasonPackMatch.Groups["season"].Value);
                result.FullSeason = true;
                result.EpisodeNumbers = Array.Empty<int>();
                result.SeriesTitle = ExtractSeriesTitle(title, seasonPackMatch);
                return true;
            }

            return false;
        }

        private static bool TryParseDaily(string title, ParsedEpisodeInfo result)
        {
            foreach (var regex in DailyEpisodeRegex)
            {
                var match = regex.Match(title);
                if (match.Success)
                {
                    var year = int.Parse(match.Groups["airyear"].Value);
                    var month = int.Parse(match.Groups["airmonth"].Value);
                    var day = int.Parse(match.Groups["airday"].Value);

                    if (year >= 1900 && year <= 2100 && month >= 1 && month <= 12 && day >= 1 && day <= 31)
                    {
                        try
                        {
                            var airDate = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
                            result.AirDate = airDate.ToString("yyyy-MM-dd");
                            result.SeriesTitle = ExtractSeriesTitle(title, match);
                            return true;
                        }
                        catch (ArgumentOutOfRangeException)
                        {
                            continue;
                        }
                    }
                }
            }

            return false;
        }

#pragma warning disable S3776 // Cognitive complexity is acceptable for parser methods
        private static bool TryParseAnime(string title, ParsedEpisodeInfo result)
        {
#pragma warning restore S3776
            foreach (var regex in AnimeEpisodeRegex)
            {
                var match = regex.Match(title);
                if (match.Success)
                {
                    var titleGroup = match.Groups["title"];
                    var subgroupGroup = match.Groups["subgroup"];
                    var episodeGroup = match.Groups["episode"];
                    var episode2Group = match.Groups["episode2"];
                    var versionGroup = match.Groups["version"];
                    var seasonGroup = match.Groups["season"];

                    if (titleGroup.Success)
                    {
                        result.SeriesTitle = titleGroup.Value.Trim();
                    }

                    if (subgroupGroup.Success)
                    {
                        result.ReleaseGroup = subgroupGroup.Value.Trim();
                    }

                    if (seasonGroup.Success)
                    {
                        result.SeasonNumber = int.Parse(seasonGroup.Value);
                    }

                    if (episodeGroup.Success)
                    {
                        var startEpisode = int.Parse(episodeGroup.Value);

                        if (episode2Group.Success)
                        {
                            var endEpisode = int.Parse(episode2Group.Value);
                            result.AbsoluteEpisodeNumbers = Enumerable.Range(startEpisode, endEpisode - startEpisode + 1).ToArray();
                        }
                        else
                        {
                            result.AbsoluteEpisodeNumbers = new[] { startEpisode };
                        }

                        if (!seasonGroup.Success)
                        {
                            result.EpisodeNumbers = result.AbsoluteEpisodeNumbers;
                        }
                    }

                    if (versionGroup.Success)
                    {
                        result.ReleaseVersion = int.Parse(versionGroup.Value);
                    }

                    return true;
                }
            }

            return false;
        }

        private static bool TryParseSpecial(string title, ParsedEpisodeInfo result)
        {
            var match = SpecialEpisodeRegex.Match(title);
            if (match.Success)
            {
                var episodeGroup = match.Groups["episode"];
                if (episodeGroup.Success)
                {
                    result.EpisodeNumbers = new[] { int.Parse(episodeGroup.Value) };
                }
                else
                {
                    result.EpisodeNumbers = new[] { 0 };
                }

                result.SeasonNumber = 0;
                result.SeriesTitle = ExtractSeriesTitle(title, match);
                return true;
            }

            return false;
        }

        private static string ExtractSeriesTitle(string title, Match match)
        {
            var index = match.Index;
            if (index <= 0)
            {
                return title.Trim();
            }

            var seriesTitle = title.Substring(0, index).Trim();
            seriesTitle = Regex.Replace(seriesTitle, @"[\._-]+$", string.Empty, RegexOptions.None, RegexTimeout);
            seriesTitle = seriesTitle.Replace('.', ' ').Replace('_', ' ').Trim();

            return seriesTitle;
        }

        private static void ParseAdditionalInfo(string title, ParsedEpisodeInfo result)
        {
            result.Quality = QualityParser.ParseQuality(title);
            result.Languages = LanguageParser.ParseLanguages(title);
            result.StreamingSource = ParseStreamingSource(title);

            if (result.ReleaseGroup.IsNullOrWhiteSpace())
            {
                result.ReleaseGroup = ReleaseGroupParser.ParseReleaseGroup(title);
            }

            var hashMatch = Regex.Match(title, @"\[(?<hash>[a-f0-9]{8})\]", RegexOptions.IgnoreCase, RegexTimeout);
            if (hashMatch.Success)
            {
                result.ReleaseHash = hashMatch.Groups["hash"].Value;
            }
        }

        public static StreamingSource ParseStreamingSource(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return StreamingSource.Unknown;
            }

            foreach (var kvp in StreamingSourceMap)
            {
                if (Regex.IsMatch(title, $@"(?:^|[\s._-]){Regex.Escape(kvp.Key)}(?:[\s._-]|$)", RegexOptions.IgnoreCase, RegexTimeout))
                {
                    return kvp.Value;
                }
            }

            return StreamingSource.Unknown;
        }

        public static bool IsFullSeason(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return false;
            }

            var result = ParseEpisodeTitle(title);
            return result?.FullSeason == true;
        }

        public static bool IsSeasonPack(string title)
        {
            return IsFullSeason(title);
        }
    }
}
