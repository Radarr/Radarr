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
        [TestCase("fr-CA")]
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
    }
}
