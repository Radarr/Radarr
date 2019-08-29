using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Parser
{
    public class IsoLanguage
    {
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }
        public List<string> AltCodes = new List<string>();
        public RuList<string> RusCodes = new RuList<string>();
        public Language Language { get; set; }

        public IsoLanguage(List<string> twoLetterCode, string threeLetterCode, Language language)
        {
            TwoLetterCode = twoLetterCode;
            ThreeLetterCode = threeLetterCode;
            Language = language;
        }

        public IsoLanguage(RuList<string> twoLetterCodes, string threeLetterCode, Language language)
        {
            TwoLetterCode = twoLetterCodes.First();
            twoLetterCodes.RemoveAt(0);
            ThreeLetterCode = threeLetterCode;
            Language = language;
            AltCodes.AddRange(twoLetterCodes);
            RusCodes.AddRange(twoLetterCode);
        }

    }
}
