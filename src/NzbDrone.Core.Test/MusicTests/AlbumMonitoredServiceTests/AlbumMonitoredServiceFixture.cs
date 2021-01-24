using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MusicTests.BookMonitoredServiceTests
{
    [TestFixture]
    public class SetBookMontitoredFixture : CoreTest<BookMonitoredService>
    {
        private Author _author;
        private List<Book> _books;

        [SetUp]
        public void Setup()
        {
            const int books = 4;

            _author = Builder<Author>.CreateNew()
                                     .Build();

            _books = Builder<Book>.CreateListOfSize(books)
                                        .All()
                                        .With(e => e.Monitored = true)
                                        .With(e => e.ReleaseDate = DateTime.UtcNow.AddDays(-7))

                                        //Future
                                        .TheFirst(1)
                                        .With(e => e.ReleaseDate = DateTime.UtcNow.AddDays(7))

                                        //Future/TBA
                                        .TheNext(1)
                                        .With(e => e.ReleaseDate = null)
                                        .Build()
                                        .ToList();

            Mocker.GetMock<IBookService>()
                  .Setup(s => s.GetBooksByAuthor(It.IsAny<int>()))
                  .Returns(_books);

            Mocker.GetMock<IBookService>()
                .Setup(s => s.GetAuthorBooksWithFiles(It.IsAny<Author>()))
                .Returns(new List<Book>());
        }

        [Test]
        public void should_be_able_to_monitor_author_without_changing_books()
        {
            Subject.SetBookMonitoredStatus(_author, null);

            Mocker.GetMock<IAuthorService>()
                  .Verify(v => v.UpdateAuthor(It.IsAny<Author>()), Times.Once());

            Mocker.GetMock<IBookService>()
                  .Verify(v => v.UpdateMany(It.IsAny<List<Book>>()), Times.Never());
        }

        [Test]
        public void should_be_able_to_monitor_books_when_passed_in_author()
        {
            var booksToMonitor = new List<string> { _books.First().ForeignBookId };

            Subject.SetBookMonitoredStatus(_author, new MonitoringOptions { Monitored = true, BooksToMonitor = booksToMonitor });

            Mocker.GetMock<IAuthorService>()
                .Verify(v => v.UpdateAuthor(It.IsAny<Author>()), Times.Once());

            VerifyMonitored(e => e.ForeignBookId == _books.First().ForeignBookId);
            VerifyNotMonitored(e => e.ForeignBookId != _books.First().ForeignBookId);
        }

        [Test]
        public void should_be_able_to_monitor_all_books()
        {
            Subject.SetBookMonitoredStatus(_author, new MonitoringOptions { Monitor = MonitorTypes.All });

            Mocker.GetMock<IBookService>()
                  .Verify(v => v.UpdateMany(It.Is<List<Book>>(l => l.All(e => e.Monitored))));
        }

        [Test]
        public void should_be_able_to_monitor_new_books_only()
        {
            var monitoringOptions = new MonitoringOptions
            {
                Monitor = MonitorTypes.Future
            };

            Subject.SetBookMonitoredStatus(_author, monitoringOptions);

            VerifyMonitored(e => e.ReleaseDate.HasValue && e.ReleaseDate.Value.After(DateTime.UtcNow));
            VerifyMonitored(e => !e.ReleaseDate.HasValue);
            VerifyNotMonitored(e => e.ReleaseDate.HasValue && e.ReleaseDate.Value.Before(DateTime.UtcNow));
        }

        private void VerifyMonitored(Func<Book, bool> predicate)
        {
            Mocker.GetMock<IBookService>()
                .Verify(v => v.UpdateMany(It.Is<List<Book>>(l => l.Where(predicate).All(e => e.Monitored))));
        }

        private void VerifyNotMonitored(Func<Book, bool> predicate)
        {
            Mocker.GetMock<IBookService>()
                .Verify(v => v.UpdateMany(It.Is<List<Book>>(l => l.Where(predicate).All(e => !e.Monitored))));
        }
    }
}
