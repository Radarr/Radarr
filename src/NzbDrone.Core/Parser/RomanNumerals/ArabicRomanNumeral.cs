namespace NzbDrone.Core.Parser.RomanNumerals
{
    public class ArabicRomanNumeral
    {
        public ArabicRomanNumeral(int arabicNumeral, string arabicNumeralAsString, string romanNumeral)
        {
            ArabicNumeral = arabicNumeral;
            ArabicNumeralAsString = arabicNumeralAsString;
            RomanNumeral = romanNumeral;
        }

        public int ArabicNumeral { get; private set; }
        public string ArabicNumeralAsString { get; private set; }
        public string RomanNumeral { get; private set; }

        public string RomanNumeralLowerCase => RomanNumeral.ToLower();
    }
}
