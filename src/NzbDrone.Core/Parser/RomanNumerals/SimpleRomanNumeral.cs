namespace NzbDrone.Core.Parser.RomanNumerals
{
    public class SimpleRomanNumeral
    {
        public SimpleRomanNumeral(string numeral)
        {
            Numeral = numeral;
        }

        public string Numeral { get; private set; }
        public string NumeralLowerCase => Numeral.ToLower();
    }
}
