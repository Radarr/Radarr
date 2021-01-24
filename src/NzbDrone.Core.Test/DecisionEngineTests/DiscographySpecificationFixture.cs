using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine.Specifications;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class DiscographySpecificationFixture : CoreTest<DiscographySpecification>
    {
        private RemoteBook _remoteBook;

        [SetUp]
        public void Setup()
        {
            var author = Builder<Author>.CreateNew().With(s => s.Id = 1234).Build();
            _remoteBook = new RemoteBook
            {
                ParsedBookInfo = new ParsedBookInfo
                {
                    Discography = true
                },
                Books = Builder<Book>.CreateListOfSize(3)
                                           .All()
                                           .With(e => e.ReleaseDate = DateTime.UtcNow.AddDays(-8))
                                           .With(s => s.AuthorId = author.Id)
                                           .BuildList(),
                Author = author,
                Release = new ReleaseInfo
                {
                    Title = "Author.Discography.1978.2005.FLAC-RlsGrp"
                }
            };

            Mocker.GetMock<IBookService>().Setup(s => s.BooksBetweenDates(It.IsAny<DateTime>(), It.IsAny<DateTime>(), false))
                                             .Returns(new List<Book>());
        }

        [Test]
        public void should_return_true_if_is_not_a_discography()
        {
            _remoteBook.ParsedBookInfo.Discography = false;
            _remoteBook.Books.Last().ReleaseDate = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_true_if_all_books_have_released()
        {
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_return_false_if_one_book_has_not_released()
        {
            _remoteBook.Books.Last().ReleaseDate = DateTime.UtcNow.AddDays(+2);
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_return_false_if_an_book_does_not_have_an_release_date()
        {
            _remoteBook.Books.Last().ReleaseDate = null;
            Subject.IsSatisfiedBy(_remoteBook, null).Accepted.Should().BeFalse();
        }
    }
}
