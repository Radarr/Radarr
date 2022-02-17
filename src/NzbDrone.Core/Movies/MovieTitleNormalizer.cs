using System.Collections.Generic;

namespace NzbDrone.Core.Movies
{
    public static class MovieTitleNormalizer
    {
        private static readonly Dictionary<int, string> PreComputedTitles = new ()
        {
            { 387354, "a to z" },
            { 1212922, "a to z" },
            { 888700, "a to z the first alphabet" },
            { 101273, "a to zeppelin the story of led zeppelin" },
        };

        public static string Normalize(string title, int tmdbId)
        {
            if (PreComputedTitles.TryGetValue(tmdbId, out var value))
            {
                return value;
            }

            return Parser.Parser.NormalizeTitle(title).ToLower();
        }
    }
}
