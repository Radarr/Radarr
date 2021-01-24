using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetAuthorFixture : CoreTest<ParsingService>
    {
        [Test]
        public void should_use_passed_in_title_when_it_cannot_be_parsed()
        {
            const string title = "30 Rock";

            Subject.GetAuthor(title);

            Mocker.GetMock<IAuthorService>()
                  .Verify(s => s.FindByName(title), Times.Once());
        }

        [Test]
        public void should_use_parsed_author_title()
        {
            const string title = "30 Rock - Get Some [FLAC]";

            Subject.GetAuthor(title);

            Mocker.GetMock<IAuthorService>()
                  .Verify(s => s.FindByName(Parser.Parser.ParseBookTitle(title).AuthorName), Times.Once());
        }
    }
}
