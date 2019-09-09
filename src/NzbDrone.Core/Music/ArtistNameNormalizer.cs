using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{

    public static class ArtistNameNormalizer
    {
        private readonly static Dictionary<string, string> PreComputedTitles = new Dictionary<string, string>
                                                                     {
                                                                         { "281588", "a to z" },
                                                                         { "266757", "ad trials triumph early church" },
                                                                         { "289260", "ad bible continues"}
                                                                     };

        public static string Normalize(string title, string mbID)
        {
            if (PreComputedTitles.ContainsKey(mbID))
            {
                return PreComputedTitles[mbID];
            }

            return Parser.Parser.NormalizeTitle(title).ToLower();
        }
    }
}
