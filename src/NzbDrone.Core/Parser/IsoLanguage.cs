
namespace NzbDrone.Core.Parser
{
    public class IsoLanguage
    {
        public string TwoLetterCode { get; set; }
        public string ThreeLetterCode { get; set; }

        public IsoLanguage(string twoLetterCode, string threeLetterCode)
        {
            TwoLetterCode = twoLetterCode;
            ThreeLetterCode = threeLetterCode;
        }
    }
}
