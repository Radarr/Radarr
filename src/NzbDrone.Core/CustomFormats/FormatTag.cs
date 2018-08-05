using System;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class FormatTag
    {
        public string Raw { get; set; }
        public TagType TagType { get; set; }
        public TagModifier TagModifier { get; set; }
        public object Value { get; set; }

        public static Regex QualityTagRegex = new Regex(@"^(?<type>R|S|M|E|L|C|I|G)(_((?<m_r>R)|(?<m_re>RE)|(?<m_n>N)){1,3})?_(?<value>.*)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static Regex SizeTagRegex = new Regex(@"(?<min>\d+(\.\d+)?)\s*<>\s*(?<max>\d+(\.\d+)?)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public FormatTag(string raw)
        {
            Raw = raw;

            var match = QualityTagRegex.Match(raw);
            if (!match.Success)
            {
                throw new ArgumentException("Quality Tag is not in the correct format!");
            }

            ParseRawMatch(match);
        }

        private FormatTag()
        {

        }

        public bool DoesItMatch(ParsedMovieInfo movieInfo)
        {
            var match = DoesItMatchWithoutMods(movieInfo);
            if (TagModifier.HasFlag(TagModifier.Not)) match = !match;
            return match;
        }

        private bool DoesItMatchWithoutMods(ParsedMovieInfo movieInfo)
        {
            switch (TagType)
            {
                case TagType.Edition:
                case TagType.Custom:
                    string compared = null;
                    if (TagType == TagType.Custom)
                    {
                        compared = movieInfo.SimpleReleaseTitle;
                    }
                    else
                    {
                        compared = movieInfo.Edition;
                    }
                    if (TagModifier.HasFlag(TagModifier.Regex))
                    {
                        Regex regexValue = (Regex) Value;
                        return regexValue.IsMatch(compared);
                    }
                    else
                    {
                        string stringValue = (string) Value;
                        return compared.ToLower().Contains(stringValue.Replace(" ", string.Empty).ToLower());
                    }
                case TagType.Language:
                    return movieInfo.Languages.Contains((Language)Value);
                case TagType.Resolution:
                    return movieInfo.Quality.Resolution == (Resolution) Value;
                case TagType.Modifier:
                    return movieInfo.Quality.Modifier == (Modifier) Value;
                case TagType.Source:
                    return movieInfo.Quality.Source == (Source) Value;
                case TagType.Size:
                    var size = (movieInfo.ExtraInfo.GetValueOrDefault("Size", 0.0) as long?) ?? 0;
                    var tuple = Value as (long, long)? ?? (0, 0);
                    return size > tuple.Item1 && size < tuple.Item2;
                case TagType.Indexer:
                    return (movieInfo.ExtraInfo.GetValueOrDefault("IndexerFlags") as IndexerFlags?)?.HasFlag((IndexerFlags) Value) == true;
                default:
                    return false;
            }
        }

        private void ParseRawMatch(Match match)
        {
            var type = match.Groups["type"].Value.ToLower();
            var value = match.Groups["value"].Value.ToLower();

            if (match.Groups["m_re"].Success) TagModifier |= TagModifier.AbsolutelyRequired;
            if (match.Groups["m_r"].Success) TagModifier |= TagModifier.Regex;
            if (match.Groups["m_n"].Success) TagModifier |= TagModifier.Not;

            switch (type)
            {
                case "r":
                    TagType = TagType.Resolution;
                    switch (value)
                    {
                        case "2160":
                            Value = Resolution.R2160P;
                            break;
                        case "1080":
                            Value = Resolution.R1080P;
                            break;
                        case "720":
                            Value = Resolution.R720P;
                            break;
                        case "576":
                            Value = Resolution.R576P;
                            break;
                        case "480":
                            Value = Resolution.R480P;
                            break;
                    }
                    break;
                case "s":
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
                    }
                    break;
                case "m":
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
                    }
                    break;
                case "e":
                    TagType = TagType.Edition;
                    if (TagModifier.HasFlag(TagModifier.Regex))
                    {
                        Value = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        Value = value;
                    }
                    break;
                case "l":
                    TagType = TagType.Language;
                    Value = Parser.LanguageParser.ParseLanguages(value).First();
                    break;
                case "i":
                    TagType = TagType.Indexer;
                    var flagValues = Enum.GetValues(typeof(IndexerFlags));

                    foreach (IndexerFlags flagValue in flagValues)
                    {
                        var flagString = flagValue.ToString();
                        if (flagString.ToLower().Replace("_", string.Empty) != value.ToLower().Replace("_", string.Empty)) continue;
                        Value = flagValue;
                        break;
                    }

                    break;
                case "g":
                    TagType = TagType.Size;
                    var matches = SizeTagRegex.Match(value);
                    var min = double.Parse(matches.Groups["min"].Value, CultureInfo.InvariantCulture);
                    var max = double.Parse(matches.Groups["max"].Value, CultureInfo.InvariantCulture);
                    Value = (min.Gigabytes(), max.Gigabytes());
                    break;
                case "c":
                default:
                    TagType = TagType.Custom;
                    if (TagModifier.HasFlag(TagModifier.Regex))
                    {
                        Value = new Regex(value, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    }
                    else
                    {
                        Value = value;
                    }
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

    public enum Resolution
    {
        Unknown = 0,
        R480P = 480,
        R576P = 576,
        R720P = 720,
        R1080P = 1080,
        R2160P = 2160
    }

    public enum Source
    {
        UNKNOWN = 0,
        CAM,
        TELESYNC,
        TELECINE,
        WORKPRINT,
        DVD,
        TV,
        WEBDL,
        BLURAY
    }

    public enum Modifier
    {
        NONE = 0,
        REGIONAL,
        SCREENER,
        RAWHD,
        BRDISK,
        REMUX
    }
}
