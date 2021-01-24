using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedHistoryItemsFixture : DbTest<CleanupOrphanedHistoryItems, History.History>
    {
        private Author _author;
        private Book _book;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>.CreateNew()
                                     .BuildNew();

            _book = Builder<Book>.CreateNew()
                .BuildNew();
        }

        private void GivenAuthor()
        {
            Db.Insert(_author);
        }

        private void GivenBook()
        {
            Db.Insert(_book);
        }

        [Test]
        public void should_delete_orphaned_items_by_author()
        {
            GivenBook();

            var history = Builder<History.History>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.BookId = _book.Id)
                                                  .BuildNew();
            Db.Insert(history);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_delete_orphaned_items_by_book()
        {
            GivenAuthor();

            var history = Builder<History.History>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.AuthorId = _author.Id)
                                                  .BuildNew();
            Db.Insert(history);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_data_by_author()
        {
            GivenAuthor();
            GivenBook();

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .All()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.BookId = _book.Id)
                                                  .TheFirst(1)
                                                  .With(h => h.AuthorId = _author.Id)
                                                  .BuildListOfNew();

            Db.InsertMany(history);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.AuthorId == _author.Id);
        }

        [Test]
        public void should_not_delete_unorphaned_data_by_book()
        {
            GivenAuthor();
            GivenBook();

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .All()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.AuthorId = _author.Id)
                                                  .TheFirst(1)
                                                  .With(h => h.BookId = _book.Id)
                                                  .BuildListOfNew();

            Db.InsertMany(history);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.BookId == _book.Id);
        }
    }
}
