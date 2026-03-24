using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Parser;

public static class ReleaseGroupParser
{
    private static readonly Regex ReleaseGroupRegex = new (@"-(?<releasegroup>[a-z0-9]+(?<part2>-[a-z0-9]+)?(?!.+?(?:480p|576p|720p|1080p|2160p)))(?<!(?:WEB-(DL|Rip)|Blu-Ray|480p|576p|720p|1080p|2160p|DTS-HD|DTS-X|DTS-MA|DTS-ES|-ES|-EN|-CAT|-ENG|-JAP|-GER|-FRA|-FRE|-ITA|-HDRip|\d{1,2}-bit|[ ._]\d{4}-\d{2}|-\d{2}|tmdb(id)?-(?<tmdbid>\d+)|(?<imdbid>tt\d{7,8}))(?:\k<part2>)?)(?:\b|[-._ ]|$)|[-._ ]\[(?<releasegroup>[a-z0-9]+)\]$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex InvalidReleaseGroupRegex = new (@"^([se]\d+|[0-9a-f]{8})$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AnimeReleaseGroupRegex = new (@"^(?:\[(?<subgroup>(?!\s).+?(?<!\s))\](?:_|-|\s|\.)?)",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // Handle Exception Release Groups that don't follow -RlsGrp; Manual List
    // name only...be very careful with this last; high chance of false positives
    private static readonly Regex ExceptionReleaseGroupRegexExact = new (@"\b(?<releasegroup>KRaLiMaRKo|E\.N\.D|D\-Z0N3|Koten_Gars|BluDragon|ZØNEHD|HQMUX|VARYG|YIFY|YTS(.(MX|LT|AG))?|TMd|Eml HDTeam|LMain|DarQ|BEN THE MEN|TAoE|QxR|126811)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    // groups whose releases end with RlsGroup) or RlsGroup]
    private static readonly Regex ExceptionReleaseGroupRegex = new (@"(?<=[._ \[])(?<releasegroup>(Silence|afm72|Panda|Ghost|MONOLITH|Tigole|Joy|ImE|UTR|t3nzin|Anime Time|Project Angel|Hakata Ramen|HONE|GiLG|Vyndros|SEV|Garshasp|Kappa|Natty|RCVR|SAMPA|YOGI|r00t|EDGE2020|RZeroX|FreetheFish|Anna|Bandi|Qman|theincognito|HDO|DusIctv|DHD|CtrlHD|-ZR-|ADC|XZVN|RH|Kametsu)(?=\]|\)))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly RegexReplace CleanReleaseGroupRegex = new (@"(-(RP|1|NZBGeek|Obfuscated|Obfuscation|Scrambled|sample|Pre|postbot|xpost|Rakuv[a-z0-9]*|WhiteRev|BUYMORE|AsRequested|AlternativeToRequested|GEROV|Z0iDS3N|Chamele0n|4P|4Planet|AlteZachen|RePACKPOST))+$",
        string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static string ParseReleaseGroup(string title)
    {
        title = title.Trim();
        title = FileExtensions.RemoveFileExtension(title);
        foreach (var replace in ParserCommon.PreSubstitutionRegex)
        {
            if (replace.TryReplace(ref title))
            {
                break;
            }
        }

        title = ParserCommon.WebsitePrefixRegex.Replace(title);
        title = ParserCommon.CleanTorrentSuffixRegex.Replace(title);

        var animeMatch = AnimeReleaseGroupRegex.Match(title);

        if (animeMatch.Success)
        {
            return animeMatch.Groups["subgroup"].Value;
        }

        title = CleanReleaseGroupRegex.Replace(title);

        var exceptionExactMatch = ExceptionReleaseGroupRegexExact.Matches(title);

        if (exceptionExactMatch.Count != 0)
        {
            return exceptionExactMatch.OfType<Match>().Last().Groups["releasegroup"].Value;
        }

        var exceptionReleaseGroupRegex = ExceptionReleaseGroupRegex.Matches(title);

        if (exceptionReleaseGroupRegex.Count != 0)
        {
            return exceptionReleaseGroupRegex.OfType<Match>().Last().Groups["releasegroup"].Value;
        }

        var matches = ReleaseGroupRegex.Matches(title);

        if (matches.Count != 0)
        {
            var group = matches.OfType<Match>().Last().Groups["releasegroup"].Value;

            if (int.TryParse(group, out _))
            {
                return null;
            }

            if (InvalidReleaseGroupRegex.IsMatch(group))
            {
                return null;
            }

            return group;
        }

        return null;
    }
}
