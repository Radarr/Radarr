using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.CustomFormats
{
    public class CustomFormatMatchResult
    {
        public CustomFormat CustomFormat { get; set; }

        public List<FormatTagMatchesGroup> GroupMatches { get; set; }

        public bool GoodMatch => GroupMatches.All(g => g.DidMatch);
    }
}
