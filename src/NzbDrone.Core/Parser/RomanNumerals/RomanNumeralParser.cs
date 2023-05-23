using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Parser.RomanNumerals
{
    public static class RomanNumeralParser
    {
        private const int DICTIONARY_PREPOPULATION_SIZE = 20;

        private static HashSet<ArabicRomanNumeral> _arabicRomanNumeralsMapping;

        private static Dictionary<SimpleArabicNumeral, SimpleRomanNumeral> _simpleArabicNumeralMappings;

        static RomanNumeralParser()
        {
            PopluateDictionariesReasonablyLarge();
        }

        private static void PopluateDictionariesReasonablyLarge()
        {
            if (_simpleArabicNumeralMappings != null || _arabicRomanNumeralsMapping != null)
            {
                return;
            }

            _arabicRomanNumeralsMapping = new HashSet<ArabicRomanNumeral>();
            _simpleArabicNumeralMappings = new Dictionary<SimpleArabicNumeral, SimpleRomanNumeral>();
            foreach (int arabicNumeral in Enumerable.Range(1, DICTIONARY_PREPOPULATION_SIZE + 1))
            {
                GenerateRomanNumerals(arabicNumeral, out var romanNumeralAsString, out var arabicNumeralAsString);
                ArabicRomanNumeral arm = new ArabicRomanNumeral(arabicNumeral, arabicNumeralAsString, romanNumeralAsString);
                _arabicRomanNumeralsMapping.Add(arm);

                SimpleArabicNumeral sam = new SimpleArabicNumeral(arabicNumeral);
                SimpleRomanNumeral srm = new SimpleRomanNumeral(romanNumeralAsString);
                _simpleArabicNumeralMappings.Add(sam, srm);
            }
        }

        private static void GenerateRomanNumerals(int arabicNumeral, out string romanNumeral, out string arabicNumeralAsString)
        {
            RomanNumeral romanNumeralObject = new RomanNumeral(arabicNumeral);
            romanNumeral = romanNumeralObject.ToRomanNumeral();
            arabicNumeralAsString = Convert.ToString(arabicNumeral);
        }

        private static HashSet<ArabicRomanNumeral> GenerateAdditionalMappings(int offset, int length)
        {
            HashSet<ArabicRomanNumeral> additionalArabicRomanNumerals = new HashSet<ArabicRomanNumeral>();
            foreach (int arabicNumeral in Enumerable.Range(offset, length))
            {
                GenerateRomanNumerals(arabicNumeral, out var romanNumeral, out var arabicNumeralAsString);
                ArabicRomanNumeral arm = new ArabicRomanNumeral(arabicNumeral, arabicNumeralAsString, romanNumeral);
                additionalArabicRomanNumerals.Add(arm);
            }

            return additionalArabicRomanNumerals;
        }

        public static HashSet<ArabicRomanNumeral> GetArabicRomanNumeralsMapping(int upToArabicNumber = DICTIONARY_PREPOPULATION_SIZE)
        {
            if (upToArabicNumber == DICTIONARY_PREPOPULATION_SIZE)
            {
                return new HashSet<ArabicRomanNumeral>(_arabicRomanNumeralsMapping.Take(upToArabicNumber));
            }

            if (upToArabicNumber < DICTIONARY_PREPOPULATION_SIZE)
            {
                return
                    (HashSet<ArabicRomanNumeral>)new HashSet<ArabicRomanNumeral>(_arabicRomanNumeralsMapping).Take(upToArabicNumber);
            }

            if (upToArabicNumber >= DICTIONARY_PREPOPULATION_SIZE)
            {
                if (_arabicRomanNumeralsMapping.Count >= upToArabicNumber)
                {
                    return new HashSet<ArabicRomanNumeral>(_arabicRomanNumeralsMapping.Take(upToArabicNumber));
                }

                HashSet<ArabicRomanNumeral> largerMapping = GenerateAdditionalMappings(DICTIONARY_PREPOPULATION_SIZE + 1, upToArabicNumber);
                _arabicRomanNumeralsMapping = (HashSet<ArabicRomanNumeral>)_arabicRomanNumeralsMapping.Union(largerMapping);
            }

            return _arabicRomanNumeralsMapping;
        }

        public static Dictionary<SimpleArabicNumeral, SimpleRomanNumeral> GetArabicRomanNumeralAsDictionary(
            int upToArabicNumer = DICTIONARY_PREPOPULATION_SIZE)
        {
            Func
            <Dictionary<SimpleArabicNumeral, SimpleRomanNumeral>, int,
                Dictionary<SimpleArabicNumeral, SimpleRomanNumeral>> take =
                (mapping, amountToTake) =>
                    new Dictionary<SimpleArabicNumeral, SimpleRomanNumeral>(
                        mapping.Take(amountToTake).ToDictionary(key => key.Key, value => value.Value));
            if (upToArabicNumer == DICTIONARY_PREPOPULATION_SIZE)
            {
                return take(_simpleArabicNumeralMappings, upToArabicNumer);
            }

            if (upToArabicNumer > DICTIONARY_PREPOPULATION_SIZE)
            {
                if (_simpleArabicNumeralMappings.Count >= upToArabicNumer)
                {
                    return take(_simpleArabicNumeralMappings, upToArabicNumer);
                }

                var moreSimpleNumerals = GenerateAdditionalSimpleNumerals(DICTIONARY_PREPOPULATION_SIZE, upToArabicNumer);
                _simpleArabicNumeralMappings =
                    (Dictionary<SimpleArabicNumeral, SimpleRomanNumeral>)_simpleArabicNumeralMappings.Union(moreSimpleNumerals);
                return take(_simpleArabicNumeralMappings, _arabicRomanNumeralsMapping.Count);
            }

            if (upToArabicNumer < DICTIONARY_PREPOPULATION_SIZE)
            {
                return take(_simpleArabicNumeralMappings, upToArabicNumer);
            }

            return _simpleArabicNumeralMappings;
        }

        private static Dictionary<SimpleArabicNumeral, SimpleRomanNumeral> GenerateAdditionalSimpleNumerals(int offset,
            int length)
        {
            Dictionary<SimpleArabicNumeral, SimpleRomanNumeral> moreNumerals = new Dictionary<SimpleArabicNumeral, SimpleRomanNumeral>();
            foreach (int arabicNumeral in Enumerable.Range(offset, length))
            {
                GenerateRomanNumerals(arabicNumeral, out var romanNumeral, out _);
                SimpleArabicNumeral san = new SimpleArabicNumeral(arabicNumeral);
                SimpleRomanNumeral srn = new SimpleRomanNumeral(romanNumeral);
                moreNumerals.Add(san, srn);
            }

            return moreNumerals;
        }
    }
}
