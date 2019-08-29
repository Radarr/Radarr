using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Parser
{
    public class IsoLanguage
    {
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
        public string RussianLetterCode { get; set; }
        public List<string> AltCodes = new List<string>();
        public Language Language { get; set; }

        public IsoLanguage(string twoLetterCode, string threeLetterCode, Language language)
        {
            TwoLetterCode = twoLetterCode;
            ThreeLetterCode = threeLetterCode;
            RussianLetterCode = RussianLetterCode
            Language = language;
        }

        public IsoLanguage(List<string> twoLetterCodes, string threeLetterCode, Language language)
        {
            TwoLetterCode = twoLetterCodes.First();
            twoLetterCodes.RemoveAt(0);
            ThreeLetterCode = threeLetterCode;
            Language = language;
            RussianLetterCode = russianLetterCode;
            AltCodes.AddRange(twoLetterCodes);
        }

    }
}
