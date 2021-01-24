using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.IndexerSearch.Definitions;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ParserTests.ParsingServiceTests
{
    [TestFixture]
    public class GetBooksFixture : CoreTest<ParsingService>
    {
        [Test]
        public void should_not_fail_if_search_criteria_contains_multiple_books_with_the_same_name()
        {
            var author = Builder<Author>.CreateNew().Build();
            var books = Builder<Book>.CreateListOfSize(2).All().With(x => x.Title = "IdenticalTitle").Build().ToList();
            var criteria = new BookSearchCriteria
            {
                Author = author,
                Books = books
            };

            var parsed = new ParsedBookInfo
            {
                BookTitle = "IdenticalTitle"
            };

            Subject.GetBooks(parsed, author, criteria).Should().BeEquivalentTo(new List<Book>());

            Mocker.GetMock<IBookService>()
                .Verify(s => s.FindByTitle(author.AuthorMetadataId, "IdenticalTitle"), Times.Once());
        }
    }
}
