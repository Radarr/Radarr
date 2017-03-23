using System;
using System.Collections.Generic;
using RomanNumeralConverter = NzbDrone.Core.Parser.RomanNumeral;

namespace NzbDrone.Core.Parser
{
    public class RomanArabicNumeralMappingBuilder
    {
        private const int SUPPORTED_ONE_BOUND_INDEX_RANGE_I = 20;

        public class RomanArabicNumeralLiteralMapping
        {
            protected RomanArabicNumeralLiteralMapping(string romanLowerCase, string romanUpperCase, int arabicAsInt)
            {
                RomanLowerCase = romanLowerCase;
                RomanUpperCase = romanUpperCase;
                ArabicAsInt = arabicAsInt;
            }

            public string RomanLowerCase { get; private set; }
            public string RomanUpperCase { get; private set; }
            public int ArabicAsInt { get; }
            public string ArabicAsString => Convert.ToString(ArabicAsInt);
        }

        private class RomanArabicNumeralLiteralMappingInstance : RomanArabicNumeralLiteralMapping
        {
            public RomanArabicNumeralLiteralMappingInstance(string romanLowerCase, string romanUpperCase,
                int arabicAsInt) : base(romanLowerCase, romanUpperCase, arabicAsInt)
            {
            }
        }

        private static IEnumerable<RomanArabicNumeralLiteralMapping> _romanArabicNumeralLiteralMapping;


        public static IEnumerable<RomanArabicNumeralLiteralMapping> BuildRomanArabicNumeralLiteralMapping()
        {
            if (_romanArabicNumeralLiteralMapping != null) return _romanArabicNumeralLiteralMapping;
            InitializeRomanArabicNumeralLiteralMapping();
            return _romanArabicNumeralLiteralMapping;
        }

        private static void InitializeRomanArabicNumeralLiteralMapping()
        {
            var minLowerBoundStartIndex = 1;
            var upperBoundEndIndex = SUPPORTED_ONE_BOUND_INDEX_RANGE_I + 1;
            _romanArabicNumeralLiteralMapping = new List<RomanArabicNumeralLiteralMapping>();
            for (var currentArabicNumeral = minLowerBoundStartIndex;
                currentArabicNumeral < upperBoundEndIndex;
                currentArabicNumeral++)
            {
                if (_romanArabicNumeralLiteralMapping != null) return;
                var romanNumeral = new RomanNumeral(currentArabicNumeral);
                var romanNumeralUpperCase = romanNumeral.ToString();
                var romanNumeralLowerCase = romanNumeralUpperCase.ToLower();
                var arabicAsInt = currentArabicNumeral;
                (_romanArabicNumeralLiteralMapping as List<RomanArabicNumeralLiteralMapping>)?.Add(
                    new RomanArabicNumeralLiteralMappingInstance(romanNumeralLowerCase, romanNumeralUpperCase,
                        arabicAsInt));
            }
        }
    }
}

