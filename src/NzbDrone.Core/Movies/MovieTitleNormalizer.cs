using System.Collections.Generic;

namespace NzbDrone.Core.Movies
{
    public static class MovieTitleNormalizer
    {
        private static readonly Dictionary<int, string> PreComputedTitles = new Dictionary<int, string>
                                                                     {
                                                                         { 999999999, "a to z" },
                                                                     };

        public static string Normalize(string title, int tmdbid)
        {
            if (PreComputedTitles.ContainsKey(tmdbid))
            {
                return PreComputedTitles[tmdbid];
            }

            return Parser.Parser.NormalizeTitle(title).ToLower();
        }
    }
}
