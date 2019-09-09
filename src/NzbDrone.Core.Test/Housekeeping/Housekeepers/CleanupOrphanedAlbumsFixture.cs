using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedAlbumsFixture : DbTest<CleanupOrphanedAlbums, Album>
    {
        [Test]
        public void should_delete_orphaned_albums()
        {
            var album = Builder<Album>.CreateNew()
                .BuildNew();

            Db.Insert(album);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_albums()
        {
            var artist = Builder<Artist>.CreateNew()
                .With(e => e.Metadata = new ArtistMetadata {Id = 1})
                .BuildNew();

            Db.Insert(artist);

            var albums = Builder<Album>.CreateListOfSize(2)
                .TheFirst(1)
                .With(e => e.ArtistMetadataId = artist.Metadata.Value.Id)
                .BuildListOfNew();

            Db.InsertMany(albums);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(e => e.ArtistMetadataId == artist.Metadata.Value.Id);
        }
    }
}
