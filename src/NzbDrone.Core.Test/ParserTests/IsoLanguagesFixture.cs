using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public void should_return_iso_language_for_English(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.English);
        }

        [TestCase("enus")]
        [TestCase("enusa")]
        [TestCase("wo")]
        [TestCase("ca-IT")]
        public void unknown_or_invalid_code_should_return_null(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Should().Be(null);
        }

        [TestCase("pt")]
        [TestCase("por")]
        public void should_return_portuguese(string isoCode)
        {
            var result = IsoLanguages.Find(isoCode);
            result.Language.Should().Be(Language.Portuguese);
        }
    }
}
