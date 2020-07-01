using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser
{
    public static class IsoLanguages
    {
        private static readonly HashSet<IsoLanguage> All = new HashSet<IsoLanguage>
                                                           {
                                                               new IsoLanguage("en", "", "eng", Language.English),
                                                               new IsoLanguage("fr", "", "fra", Language.French),
                                                               new IsoLanguage("es", "", "spa", Language.Spanish),
                                                               new IsoLanguage("de", "", "deu", Language.German),
                                                               new IsoLanguage("it", "", "ita", Language.Italian),
                                                               new IsoLanguage("da", "", "dan", Language.Danish),
                                                               new IsoLanguage("nl", "", "nld", Language.Dutch),
                                                               new IsoLanguage("ja", "", "jpn", Language.Japanese),
                                                               new IsoLanguage("is", "", "isl", Language.Icelandic),
                                                               new IsoLanguage("zh", "", "zho", Language.Chinese),
                                                               new IsoLanguage("ru", "", "rus", Language.Russian),
                                                               new IsoLanguage("pl", "", "pol", Language.Polish),
                                                               new IsoLanguage("vi", "", "vie", Language.Vietnamese),
                                                               new IsoLanguage("sv", "", "swe", Language.Swedish),
                                                               new IsoLanguage("no", "", "nor", Language.Norwegian),
                                                               new IsoLanguage("nb", "", "nob", Language.Norwegian),
                                                               new IsoLanguage("fi", "", "fin", Language.Finnish),
                                                               new IsoLanguage("tr", "", "tur", Language.Turkish),
                                                               new IsoLanguage("pt", "", "por", Language.Portuguese),
                                                               new IsoLanguage("el", "", "ell", Language.Greek),
                                                               new IsoLanguage("ko", "", "kor", Language.Korean),
                                                               new IsoLanguage("hu", "", "hun", Language.Hungarian),
                                                               new IsoLanguage("he", "", "heb", Language.Hebrew),
                                                               new IsoLanguage("cs", "", "ces", Language.Czech)
                                                           };

        public static IsoLanguage Find(string isoCode)
        {
            var isoArray = isoCode.Split('-');

            var langCode = isoArray[0].ToLower();

            if (langCode.Length == 2)
            {
                //Lookup ISO639-1 code
                var isoLanguages = All.Where(l => l.TwoLetterCode == langCode).ToList();

                if (isoArray.Length > 1)
                {
                    isoLanguages = isoLanguages.Any(l => l.CountryCode == isoArray[1].ToLower()) ?
                        isoLanguages.Where(l => l.CountryCode == isoArray[1].ToLower()).ToList() : isoLanguages;
                }

                return isoLanguages.FirstOrDefault();
            }
            else if (langCode.Length == 3)
            {
                //Lookup ISO639-2T code
                return All.FirstOrDefault(l => l.ThreeLetterCode == langCode);
            }

            return null;
        }

        public static IsoLanguage Get(Language language)
        {
            return All.FirstOrDefault(l => l.Language == language);
        }
    }
}
