using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.Music
{

    public static class ArtistNameNormalizer
    {
        private readonly static Dictionary<int, string> PreComputedTitles = new Dictionary<int, string>
                                                                     {
                                                                         { 281588, "a to z" },
                                                                         { 266757, "ad trials triumph early church" },
                                                                         { 289260, "ad bible continues"}
                                                                     };

        public static string Normalize(string title, int iTunesId)
        {
            if (PreComputedTitles.ContainsKey(iTunesId))
            {
                return PreComputedTitles[iTunesId];
            }

            return Parser.Parser.NormalizeTitle(title).ToLower();
        }
    }
}
