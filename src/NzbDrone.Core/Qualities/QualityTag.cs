using System;
using System.Text.RegularExpressions;
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

        public bool DoesItMatch(ParsedMovieInfo movieInfo)
        {
            switch (TagType)
            {
                case TagType.Edition:
                    string stringValue = (string) Value;
                    return movieInfo.Edition.Contains(stringValue.Remove(' ').ToLower());
                default:
                    return false;
            }
        }

    }

    public enum TagType
    {
        Resolution = 1,
        Source = 2,
        Modifiers = 4,
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
}
