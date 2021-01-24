using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedBooksFixture : DbTest<CleanupOrphanedBooks, Book>
    {
        [Test]
        public void should_delete_orphaned_books()
        {
            var book = Builder<Book>.CreateNew()
                .BuildNew();

            Db.Insert(book);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_books()
        {
            var author = Builder<Author>.CreateNew()
                .With(e => e.Metadata = new AuthorMetadata { Id = 1 })
                .BuildNew();

            Db.Insert(author);

            var books = Builder<Book>.CreateListOfSize(2)
                .TheFirst(1)
                .With(e => e.AuthorMetadataId = author.Metadata.Value.Id)
                .BuildListOfNew();

            Db.InsertMany(books);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(e => e.AuthorMetadataId == author.Metadata.Value.Id);
        }
    }
}
