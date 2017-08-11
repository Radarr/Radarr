﻿using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Parser
{
    public static class IsoLanguages
    {
        private static readonly HashSet<IsoLanguage> All = new HashSet<IsoLanguage>
                                                           {
                                                               new IsoLanguage("en", "eng", Language.English),
                                                               new IsoLanguage("fr", "fra", Language.French),
                                                               new IsoLanguage("es", "spa", Language.Spanish),
                                                               new IsoLanguage("de", "deu", Language.German),
                                                               new IsoLanguage("it", "ita", Language.Italian),
                                                               new IsoLanguage("da", "dan", Language.Danish),
                                                               new IsoLanguage("nl", "nld", Language.Dutch),
                                                               new IsoLanguage("ja", "jpn", Language.Japanese),
//                                                             new IsoLanguage("", "", Language.Cantonese),
//                                                             new IsoLanguage("", "", Language.Mandarin),
                                                               new IsoLanguage("ru", "rus", Language.Russian),
                                                               new IsoLanguage("pl", "pol", Language.Polish),
                                                               new IsoLanguage("vi", "vie", Language.Vietnamese),
                                                               new IsoLanguage("sv", "swe", Language.Swedish),
                                                               new IsoLanguage("no", "nor", Language.Norwegian),
                                                               new IsoLanguage("fi", "fin", Language.Finnish),
                                                               new IsoLanguage("tr", "tur", Language.Turkish),
                                                               new IsoLanguage("pt", "por", Language.Portuguese),
//                                                             new IsoLanguage("nl", "nld", Language.Flemish),
                                                               new IsoLanguage("el", "ell", Language.Greek),
                                                               new IsoLanguage("ko", "kor", Language.Korean),
                                                               new IsoLanguage("hu", "hun", Language.Hungarian),
                                                               new IsoLanguage("he", "heb", Language.Hebrew)
                                                           };

        public static IsoLanguage Find(string isoCode)
        {
            if (isoCode.Length == 2)
            {
                //Lookup ISO639-1 code
                return All.SingleOrDefault(l => l.TwoLetterCode == isoCode);
            }
            else if (isoCode.Length == 3)
            {
                //Lookup ISO639-2T code
                return All.SingleOrDefault(l => l.ThreeLetterCode == isoCode);
            }

            return null;
        }

        public static IsoLanguage Get(Language language)
        {
            return All.SingleOrDefault(l => l.Language == language);
        }
    }
}
