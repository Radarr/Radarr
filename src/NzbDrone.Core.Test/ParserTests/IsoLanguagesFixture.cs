using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class IsoLanguagesFixture : CoreTest
    {
        [TestCase("en")]
        [TestCase("eng")]
        [TestCase("en-US")]
        [TestCase("en-GB")]
        public void should_return_iso_language_for_English(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.English);
        }

        [TestCase("enus")]
        [TestCase("enusa")]
        [TestCase("wo")]
        public void unknown_or_invalid_code_should_return_null(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().Be(null);
        }

        [TestCase("pt")]
        [TestCase("por")]
        [TestCase("pt-PT")]
        public void should_return_portuguese(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Portuguese);
        }

        [TestCase("de-AU")]
        public void should_not_return_portuguese(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().Be(null);
        }

        [TestCase("te")]
        [TestCase("tel")]
        [TestCase("te-IN")]
        public void should_return_telugu(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Telugu);
        }

        [TestCase("af")]
        [TestCase("afr")]
        [TestCase("af-ZA")]
        public void should_return_afrikaans(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Afrikaans);
        }

        [TestCase("mr")]
        [TestCase("mar")]
        [TestCase("mr-IN")]
        public void should_return_marathi(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Marathi);
        }

        [TestCase("tl")]
        [TestCase("tgl")]
        [TestCase("tl-PH")]
        public void should_return_tagalog(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Tagalog);
        }

        [TestCase("ur")]
        [TestCase("urd")]
        [TestCase("ur-PK")]
        public void should_return_urdu(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Urdu);
        }

        [TestCase("rm")]
        [TestCase("roh")]
        [TestCase("rm-CH")]
        public void should_return_romansh(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Romansh);
        }

        [TestCase("mn")]
        [TestCase("mon")]
        [TestCase("khk")]
        [TestCase("mvf")]
        [TestCase("mn-Cyrl")]
        public void should_return_mongolian(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Mongolian);
        }

        [TestCase("bn")]
        [TestCase("ben")]
        [TestCase("bn-BD")]
        [TestCase("bn-IN")]
        public void should_return_bengali(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Bengali);
        }

        [TestCase("ka")]
        [TestCase("geo")]
        [TestCase("kat")]
        [TestCase("ka-GE")]
        public void should_return_georgian(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Georgian);
        }

        [TestCase("fr-CA")]
        [TestCase("fr-ca")]
        [TestCase("FR-CA")]
        public void should_return_french_for_french_canadian(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().NotBeNull();
            result.Language.Should().Be(Language.French);
        }

        [TestCase("en-CA")]
        [TestCase("en-ca")]
        [TestCase("EN-CA")]
        public void should_return_english_for_english_canadian(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().NotBeNull();
            result.Language.Should().Be(Language.English);
        }

        [Test]
        public void french_canadian_should_map_to_same_language_as_french()
        {
            var frCA = IsoLanguages.Find("fr-CA");
            var fr = IsoLanguages.Find("fr");

            frCA.Should().NotBeNull();
            fr.Should().NotBeNull();
            frCA.Language.Should().Be(fr.Language);
        }

        [Test]
        public void english_canadian_should_map_to_same_language_as_english()
        {
            var enCA = IsoLanguages.Find("en-CA");
            var en = IsoLanguages.Find("en");

            enCA.Should().NotBeNull();
            en.Should().NotBeNull();
            enCA.Language.Should().Be(en.Language);
        }

        [TestCase("de-AT")]
        [TestCase("de-at")]
        public void should_return_german_for_german_austria(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().NotBeNull();
            result.Language.Should().Be(Language.German);
        }

        [TestCase("de-CH")]
        [TestCase("de-ch")]
        public void should_return_german_for_german_switzerland(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().NotBeNull();
            result.Language.Should().Be(Language.German);
        }

        [TestCase("zh-TW")]
        [TestCase("zh-tw")]
        public void should_return_chinese_for_chinese_taiwan(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().NotBeNull();
            result.Language.Should().Be(Language.Chinese);
        }

        [TestCase("zh-HK")]
        [TestCase("zh-hk")]
        public void should_return_chinese_for_chinese_hong_kong(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().NotBeNull();
            result.Language.Should().Be(Language.Chinese);
        }
    }
}
