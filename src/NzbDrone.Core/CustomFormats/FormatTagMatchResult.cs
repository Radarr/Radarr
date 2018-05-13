using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace NzbDrone.Core.CustomFormats
{
    public class FormatTagMatchResult
    {
        public FormatTagMatchResult()
        {
            GroupMatches = new List<FormatTagMatchesGroup>();
        }
        public CustomFormat CustomFormat { get; set; }
        public List<FormatTagMatchesGroup> GroupMatches { get; set; }
        public bool GoodMatch { get; set; }
    }

    public class FormatTagMatchesGroup
    {
        public FormatTagMatchesGroup()
        {
            Matches = new Dictionary<FormatTag, bool>();
        }

        public FormatTagMatchesGroup(TagType type, Dictionary<FormatTag, bool> matches)
        {
            Type = type;
            Matches = matches;
        }

        public TagType Type { get; set; }

        public bool DidMatch
        {
            get
            {
                return !(Matches.Any(m => m.Key.TagModifier == TagModifier.AbsolutelyRequired && m.Value == false) ||
                       Matches.All(m => m.Value == false));
            }
        }
        public Dictionary<FormatTag, bool> Matches { get; set; }
    }
}
