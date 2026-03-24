using System.Text.RegularExpressions;

namespace NzbDrone.Core.Parser;

// These are functions shared between different parser functions
// they are not intended to be used outside of them parsing.
internal static class ParserCommon
{
    internal static readonly RegexReplace[] PreSubstitutionRegex = System.Array.Empty<RegexReplace>();

    // Valid TLDs http://data.iana.org/TLD/tlds-alpha-by-domain.txt
    internal static readonly RegexReplace WebsitePrefixRegex = new (@"^(?:(?:\[|\()\s*)?(?:www\.)?[-a-z0-9-]{1,256}\.(?<!Naruto-Kun\.)(?:[a-z]{2,6}\.[a-z]{2,6}|xn--[a-z0-9-]{4,}|[a-z]{2,})\b(?:\s*(?:\]|\))|[ -]{2,})[ -]*",
        string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    internal static readonly RegexReplace WebsitePostfixRegex = new (@"(?:\[\s*)?(?:www\.)?[-a-z0-9-]{1,256}\.(?:xn--[a-z0-9-]{4,}|[a-z]{2,6})\b(?:\s*\])$",
        string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    internal static readonly RegexReplace CleanTorrentSuffixRegex = new (@"\[(?:ettv|rartv|rarbg|cttv|publichd)\]$",
        string.Empty,
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
}
