using System.Collections.Generic;

namespace NzbDrone.Core.Tv
{
    public static class MovieTitleNormalizer
    {
        private readonly static Dictionary<string, string> PreComputedTitles = new Dictionary<string, string>
                                                                     {
                                                                         { "tt_109823457098", "a to z" },
                                                                     };

        public static string Normalize(string title, string imdbid)
        {
            if (PreComputedTitles.ContainsKey(imdbid))
            {
                return PreComputedTitles[imdbid];
            }

            return Parser.Parser.NormalizeTitle(title).ToLower();
        }
    }
}
