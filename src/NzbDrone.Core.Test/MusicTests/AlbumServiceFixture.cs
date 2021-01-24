using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.BookRepositoryTests
{
    [TestFixture]
    public class BookServiceFixture : CoreTest<BookService>
    {
        private List<Book> _books;

        [SetUp]
        public void Setup()
        {
            _books = new List<Book>();
            _books.Add(new Book
            {
                Title = "ANThology",
                CleanTitle = "anthology",
            });

            _books.Add(new Book
            {
                Title = "+",
                CleanTitle = "",
            });

            Mocker.GetMock<IBookRepository>()
                .Setup(s => s.GetBooksByAuthorMetadataId(It.IsAny<int>()))
                .Returns(_books);
        }

        private void GivenSimilarBook()
        {
            _books.Add(new Book
            {
                Title = "ANThology2",
                CleanTitle = "anthology2",
            });
        }

        [TestCase("ANTholog", "ANThology")]
        [TestCase("antholoyg", "ANThology")]
        [TestCase("ANThology CD", "ANThology")]
        [TestCase("ANThology CD xxxx (Remastered) - [Oh please why do they do this?]", "ANThology")]
        [TestCase("+ (Plus) - I feel the need for redundant information in the title field", "+")]
        public void should_find_book_in_db_by_inexact_title(string title, string expected)
        {
            var book = Subject.FindByTitleInexact(0, title);

            book.Should().NotBeNull();
            book.Title.Should().Be(expected);
        }

        [TestCase("ANTholog")]
        [TestCase("antholoyg")]
        [TestCase("ANThology CD")]
        [TestCase("÷")]
        [TestCase("÷ (Divide)")]
        public void should_not_find_book_in_db_by_inexact_title_when_two_similar_matches(string title)
        {
            GivenSimilarBook();
            var book = Subject.FindByTitleInexact(0, title);

            book.Should().BeNull();
        }
    }
}
