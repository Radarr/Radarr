using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Languages
{
    public static class LanguageExtensions
    {
        public static string ToExtendedString(this IEnumerable<Language> languages)
        {
            return string.Join(", ", languages.Select(l => l.ToString()));
        }
    }
}
