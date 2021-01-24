using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class MusicParserFixture : CoreTest
    {
        //[TestCase("___▲▲▲___")]
        //[TestCase("Add N to (X)")]
        //[TestCase("Animal Collective")]
        //[TestCase("D12")]
        //[TestCase("David Sylvian[Discography]")]
        //[TestCase("Eagle-Eye Cherry")]
        //[TestCase("Erlend Øye")]
        //[TestCase("Adult.")] // Not sure if valid, not openable in Windows OS
        //[TestCase("Maroon 5")]
        //[TestCase("Moimir Papalescu & The Nihilists")]
        //[TestCase("N.W.A")]
        //[TestCase("oOoOO")]
        //[TestCase("Panic! at the Disco")]
        //[TestCase("The 5 6 7 8's")]
        //[TestCase("tUnE-yArDs")]
        //[TestCase("U2")]
        //[TestCase("Белые Братья")]
        //[TestCase("Zog Bogbean - From The Marcy Playground")]

        // TODO: Rewrite this test to something that makes sense.
        public void should_parse_author_names(string title)
        {
            Parser.Parser.ParseTitle(title).AuthorTitle.Should().Be(title);
            ExceptionVerification.IgnoreWarns();
        }
    }
}
