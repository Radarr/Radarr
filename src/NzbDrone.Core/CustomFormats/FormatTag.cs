using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.CustomFormats
{
    public class FormatTag
    {
        public static Regex QualityTagRegex = new Regex(@"^(?<type>R|S|M|E|L|C|I|G)(_((?<m_r>RX)|(?<m_re>RQ)|(?<m_n>N)){0,3})?_(?<value>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex SizeTagRegex = new Regex(@"(?<min>\d+(\.\d+)?)\s*<>\s*(?<max>\d+(\.\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // This function is needed for json deserialization to work.
        public FormatTag()
        {
        }

        public FormatTag(string raw)
        {
            Raw = raw;

            var match = QualityTagRegex.Match(raw);
            if (!match.Success)
            {
                throw new ArgumentException("Quality Tag is not in the correct format!");
            }

            ParseFormatTagString(match);
        }

        public string Raw { get; set; }
        public TagType TagType { get; set; }
        public TagModifier TagModifier { get; set; }
        public object Value { get; set; }

        public bool DoesItMatch(ParsedMovieInfo movieInfo)
        {
            var match = DoesItMatchWithoutMods(movieInfo);
            if (TagModifier.HasFlag(TagModifier.Not))
            {
                match = !match;
            }

            return match;
        }

        private bool MatchString(string compared)
        {
            if (compared == null)
            {
                return false;
            }

            if (TagModifier.HasFlag(TagModifier.Regex))
            {
                var regexValue = (Regex)Value;
                return regexValue.IsMatch(compared);
            }
            else
            {
                var stringValue = (string)Value;
                return compared.ToLower().Contains(stringValue.Replace(" ", string.Empty).ToLower());
            }
        }

        private bool DoesItMatchWithoutMods(ParsedMovieInfo movieInfo)
        {
            if (movieInfo == null)
            {
                return false;
            }

            var filename = (string)movieInfo?.ExtraInfo?.GetValueOrDefault("Filename");

            switch (TagType)
            {
                case TagType.Edition:
                    return MatchString(movieInfo.Edition);
                case TagType.Custom:
                    return MatchString(movieInfo.SimpleReleaseTitle) || MatchString(filename);
                case TagType.Language:
                    return movieInfo?.Languages?.Contains((Language)Value) ?? false;
                case TagType.Resolution:
                    return (movieInfo?.Quality?.Quality?.Resolution ?? (int)Resolution.Unknown) == (int)(Resolution)Value;
                case TagType.Modifier:
                    return (movieInfo?.Quality?.Quality?.Modifier ?? (int)Modifier.NONE) == (Modifier)Value;
                case TagType.Source:
                    return (movieInfo?.Quality?.Quality?.Source ?? (int)Source.UNKNOWN) == (Source)Value;
                case TagType.Size:
                    var size = (movieInfo?.ExtraInfo?.GetValueOrDefault("Size", 0.0) as long?) ?? 0;
                    var tuple = Value as (long, long)? ?? (0, 0);
                    return size > tuple.Item1 && size < tuple.Item2;
                case TagType.Indexer:
#if !LIBRARY
                    var flags = movieInfo?.ExtraInfo?.GetValueOrDefault("IndexerFlags") as IndexerFlags?;
                    return flags?.HasFlag((IndexerFlags)Value) == true;
#endif
                default:
                    return false;
            }
        }

        private void ParseTagModifier(Match match)
        {
            if (match.Groups["m_re"].Success)
            {
                TagModifier |= TagModifier.AbsolutelyRequired;
            }

            if (match.Groups["m_r"].Success)
            {
                TagModifier |= TagModifier.Regex;
            }

            if (match.Groups["m_n"].Success)
            {
                TagModifier |= TagModifier.Not;
            }
        }

        private void ParseResolutionType(string value)
        {
            TagType = TagType.Resolution;
            switch (value)
            {
                case "2160":
                    Value = Resolution.R2160p;
                    break;
                case "1080":
                    Value = Resolution.R1080p;
                    break;
                case "720":
                    Value = Resolution.R720p;
                    break;
                case "576":
                    Value = Resolution.R576p;
                    break;
                case "480":
                    Value = Resolution.R480p;
                    break;
                default:
                    break;
            }
        }

        private void ParseSourceType(string value)
        {
            TagType = TagType.Source;
            switch (value)
            {
                case "cam":
                    Value = Source.CAM;
                    break;
                case "telesync":
                    Value = Source.TELESYNC;
                    break;
                case "telecine":
                    Value = Source.TELECINE;
                    break;
                case "workprint":
                    Value = Source.WORKPRINT;
                    break;
                case "dvd":
                    Value = Source.DVD;
                    break;
                case "tv":
                    Value = Source.TV;
                    break;
                case "webdl":
                    Value = Source.WEBDL;
                    break;
                case "bluray":
                    Value = Source.BLURAY;
                    break;
                default:
                    break;
            }
        }

        private void ParseModifierType(string value)
        {
            TagType = TagType.Modifier;
            switch (value)
            {
                case "regional":
                    Value = Modifier.REGIONAL;
                    break;
                case "screener":
                    Value = Modifier.SCREENER;
                    break;
                case "rawhd":
                    Value = Modifier.RAWHD;
                    break;
                case "brdisk":
                    Value = Modifier.BRDISK;
                    break;
                case "remux":
                    Value = Modifier.REMUX;
                    break;
                default:
                    break;
            }
        }

        private void ParseIndexerFlagType(string value)
        {
            TagType = TagType.Indexer;
            var flagValues = Enum.GetValues(typeof(IndexerFlags));

            foreach (IndexerFlags flagValue in flagValues)
            {
                var flagString = flagValue.ToString();
                if (flagString.ToLower().Replace("_", string.Empty) != value.ToLower().Replace("_", string.Empty))
                {
                    continue;
                }

                Value = flagValue;
                break;
            }
        }

        private void ParseSizeType(string value)
        {
            TagType = TagType.Size;
            var matches = SizeTagRegex.Match(value);
            var min = double.Parse(matches.Groups["min"].Value, CultureInfo.InvariantCulture);
            var max = double.Parse(matches.Groups["max"].Value, CultureInfo.InvariantCulture);
            Value = (min.Gigabytes(), max.Gigabytes());
        }

        private void ParseString(string value)
        {
            if (TagModifier.HasFlag(TagModifier.Regex))
            {
                Value = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
            else
            {
                Value = value;
            }
        }

        private void ParseFormatTagString(Match match)
        {
            ParseTagModifier(match);

            var type = match.Groups["type"].Value.ToLower();
            var value = match.Groups["value"].Value.ToLower();

            switch (type)
            {
                case "r":
                    ParseResolutionType(value);
                    break;
                case "s":
                    ParseSourceType(value);
                    break;
                case "m":
                    ParseModifierType(value);
                    break;
                case "e":
                    TagType = TagType.Edition;
                    ParseString(value);
                    break;
                case "l":
                    TagType = TagType.Language;
                    Value = LanguageParser.ParseLanguages(value).First();
                    break;
                case "i":
#if !LIBRARY
                    ParseIndexerFlagType(value);
#endif
                    break;
                case "g":
                    ParseSizeType(value);
                    break;
                case "c":
                default:
                    TagType = TagType.Custom;
                    ParseString(value);
                    break;
            }
        }
    }

    public enum TagType
    {
        Resolution = 1,
        Source = 2,
        Modifier = 4,
        Edition = 8,
        Language = 16,
        Custom = 32,
        Indexer = 64,
        Size = 128,
    }

    [Flags]
    public enum TagModifier
    {
        Regex = 1,
        Not = 2, // Do not match
        AbsolutelyRequired = 4
    }
}
