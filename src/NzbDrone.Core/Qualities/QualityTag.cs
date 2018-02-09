using System;
using System.Text.RegularExpressions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Qualities
{
    public class QualityTag
    {
        public string Raw { get; set; }
        public TagType TagType { get; set; }
        public TagModifier TagModifier { get; set; }
        public object Value { get; set; }
        
        public QualityTag(string raw)
        {
            Raw = raw;
        }

        public bool DoesItMatch(ParsedMovieInfo movieInfo, ReleaseInfo releaseInfo)
        {
            var match = DoesItMatchWithoutMods(movieInfo, releaseInfo);
            if (TagModifier.HasFlag(TagModifier.Not)) match = !match;
            return match;
        }

        private bool DoesItMatchWithoutMods(ParsedMovieInfo movieInfo, ReleaseInfo releaseInfo)
        {
            switch (TagType)
            {
                case TagType.Edition:
                case TagType.Custom:
                    string compared = null;
                    if (TagType == TagType.Custom)
                    {
                        compared = releaseInfo.Title;
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
                        return compared.Contains(stringValue.Remove(' ').ToLower());
                    }
                case TagType.Language:
                    return movieInfo.Languages.Contains((Language)Value);
                case TagType.Resolution:
                    return movieInfo.Quality.Resolution == (Resolution) Value;
                case TagType.Modifier:
                    return movieInfo.Quality.Modifier == (Modifier) Value;
                case TagType.Source:
                    return movieInfo.Quality.Source == (Source) Value;
                default:
                    return false;
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
        Custom = 32
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
        R480P,
        R576p,
        R720p,
        R1080p,
        R2160p,
        Unknown
    }

    public enum Source
    {
        CAM,
        TELESYNC,
        TELECINE,
        DVD,
        HDTV,
        WEBDL,
        BLURAY
    }

    public enum Modifier
    {
        REGIONAL,
        SCREENER,
        RAWHD,
        BRDISK,
        REMUX
    }
}
