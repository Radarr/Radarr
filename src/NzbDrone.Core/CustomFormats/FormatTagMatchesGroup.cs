using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.CustomFormats
{
    public class FormatTagMatchesGroup
    {
        public TagType Type { get; set; }

        public Dictionary<FormatTag, bool> Matches { get; set; }

        public bool DidMatch => !(Matches.Any(m => m.Key.TagModifier.HasFlag(TagModifier.AbsolutelyRequired) && m.Value == false) ||
                                  Matches.All(m => m.Value == false));
    }
}
