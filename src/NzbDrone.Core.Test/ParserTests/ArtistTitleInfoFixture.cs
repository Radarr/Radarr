using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests
{
    [TestFixture]
    public class AuthorTitleInfoFixture : CoreTest
    {
        // TODO: Redo this test and parsed info for Books which do have a year association
        [Test]
        [Ignore("Author Don't have year association thus we dont use this currently")]
        public void should_have_year_zero_when_title_doesnt_have_a_year()
        {
            const string title = "Alien Ant Farm - TruAnt [Flac]";

            var result = Parser.Parser.ParseBookTitle(title).AuthorTitleInfo;

            result.Year.Should().Be(0);
        }

        [Test]
        [Ignore("Author Don't have year association thus we dont use this currently")]
        public void should_have_same_title_for_title_and_title_without_year_when_title_doesnt_have_a_year()
        {
            const string title = "Alien Ant Farm - TruAnt [Flac]";

            var result = Parser.Parser.ParseBookTitle(title).AuthorTitleInfo;

            result.Title.Should().Be(result.TitleWithoutYear);
        }

        [Test]
        [Ignore("Author Don't have year association thus we dont use this currently")]
        public void should_have_year_when_title_has_a_year()
        {
            const string title = "Alien Ant Farm - TruAnt [Flac]";

            var result = Parser.Parser.ParseBookTitle(title).AuthorTitleInfo;

            result.Year.Should().Be(2004);
        }

        [Test]
        [Ignore("Author Don't have year association thus we dont use this currently")]
        public void should_have_year_in_title_when_title_has_a_year()
        {
            const string title = "Alien Ant Farm - TruAnt [Flac]";

            var result = Parser.Parser.ParseBookTitle(title).AuthorTitleInfo;

            result.Title.Should().Be("House 2004");
        }

        [Test]
        [Ignore("Author Don't have year association thus we dont use this currently")]
        public void should_title_without_year_should_not_contain_year()
        {
            const string title = "Alien Ant Farm - TruAnt [Flac]";

            var result = Parser.Parser.ParseBookTitle(title).AuthorTitleInfo;

            result.TitleWithoutYear.Should().Be("House");
        }
    }
}
