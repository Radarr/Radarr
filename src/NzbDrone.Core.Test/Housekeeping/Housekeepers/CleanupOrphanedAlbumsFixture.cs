using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Music;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedAlbumsFixture : DbTest<CleanupOrphanedBooks, Book>
    {
        [Test]
        public void should_delete_orphaned_albums()
        {
            var album = Builder<Book>.CreateNew()
                .BuildNew();

            Db.Insert(album);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_albums()
        {
            var artist = Builder<Author>.CreateNew()
                .With(e => e.Metadata = new AuthorMetadata { Id = 1 })
                .BuildNew();

            Db.Insert(artist);

            var albums = Builder<Book>.CreateListOfSize(2)
                .TheFirst(1)
                .With(e => e.AuthorMetadataId = artist.Metadata.Value.Id)
                .BuildListOfNew();

            Db.InsertMany(albums);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(e => e.AuthorMetadataId == artist.Metadata.Value.Id);
        }
    }
}
