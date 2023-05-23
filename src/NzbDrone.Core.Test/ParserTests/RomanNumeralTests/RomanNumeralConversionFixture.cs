using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using NzbDrone.Core.Parser.RomanNumerals;

namespace NzbDrone.Core.Test.ParserTests.RomanNumeralTests
{
    [TestFixture]
    public class RomanNumeralConversionFixture
    {
        private const string TEST_VALUES = @"Files/ArabicRomanNumeralDictionary.JSON";

        private Dictionary<int, string> _arabicToRomanNumeralsMapping;

        [OneTimeSetUp]
        public void PopulateDictionaryWithProvenValues()
        {
            var pathToTestValues = Path.Combine(TestContext.CurrentContext.TestDirectory, Path.Combine(TEST_VALUES.Split('/')));
            _arabicToRomanNumeralsMapping =
                JsonConvert.DeserializeObject<Dictionary<int, string>>(File.ReadAllText(pathToTestValues));
        }

        [Test(Description = "Converts the supported range [1-3999] of Arabic to Roman numerals.")]
        [Order(0)]
        public void should_convert_arabic_numeral_to_roman_numeral([Range(1, 20)] int arabicNumeral)
        {
            var romanNumeral = new RomanNumeral(arabicNumeral);

            var expectedValue = _arabicToRomanNumeralsMapping[arabicNumeral];

            Assert.AreEqual(romanNumeral.ToRomanNumeral(), expectedValue);
        }

        [Test]
        [Order(1)]
        public void should_convert_roman_numeral_to_arabic_numeral([Range(1, 20)] int arabicNumeral)
        {
            var romanNumeral = new RomanNumeral(_arabicToRomanNumeralsMapping[arabicNumeral]);

            var expectecdValue = arabicNumeral;

            Assert.AreEqual(romanNumeral.ToInt(), expectecdValue);
        }
    }
}
