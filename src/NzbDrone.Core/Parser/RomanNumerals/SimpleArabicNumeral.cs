using System;

namespace NzbDrone.Core.Parser.RomanNumerals
{
    public class SimpleArabicNumeral
    {
        public SimpleArabicNumeral(int numeral)
        {
            Numeral = numeral;
        }

        public int Numeral { get; private set; }
        public string NumeralAsString => Convert.ToString(Numeral);
    }
}
