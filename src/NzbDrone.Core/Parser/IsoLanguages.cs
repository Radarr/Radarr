using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Parser
{
    public static class IsoLanguages
    {
        private static readonly HashSet<IsoLanguage> All = new HashSet<IsoLanguage>
                                                           {
                                                               new IsoLanguage("en", "eng"),
                                                               new IsoLanguage("fr", "fra"),
                                                               new IsoLanguage("es", "spa"),
                                                               new IsoLanguage("de", "deu"),
                                                               new IsoLanguage("it", "ita"),
                                                               new IsoLanguage("da", "dan"),
                                                               new IsoLanguage("nl", "nld"),
                                                               new IsoLanguage("ja", "jpn"),
                                                               new IsoLanguage("is", "isl"),
                                                               new IsoLanguage("zh", "zho"),
                                                               new IsoLanguage("ru", "rus"),
                                                               new IsoLanguage("pl", "pol"),
                                                               new IsoLanguage("vi", "vie"),
                                                               new IsoLanguage("sv", "swe"),
                                                               new IsoLanguage("no", "nor"),
                                                               new IsoLanguage("nb", "nob"), // Norwegian Bokmål
                                                               new IsoLanguage("fi", "fin"),
                                                               new IsoLanguage("tr", "tur"),
                                                               new IsoLanguage("pt", "por"),
                                                               new IsoLanguage("el", "ell"),
                                                               new IsoLanguage("ko", "kor"),
                                                               new IsoLanguage("hu", "hun"),
                                                               new IsoLanguage("he", "heb"),
                                                               new IsoLanguage("lt", "lit"),
                                                               new IsoLanguage("cs", "ces")
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
    }
}
