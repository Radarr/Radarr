using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedHistoryItemsFixture : DbTest<CleanupOrphanedHistoryItems, History.History>
    {
        private Artist _artist;
        private Album _album;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>.CreateNew()
                                     .BuildNew();

            _album = Builder<Album>.CreateNew()
                .BuildNew();
        }

        private void GivenArtist()
        {
            Db.Insert(_artist);
        }

        private void GivenAlbum()
        {
            Db.Insert(_album);
        }

        [Test]
        public void should_delete_orphaned_items_by_artist()
        {
            GivenAlbum();

            var history = Builder<History.History>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.AlbumId = _album.Id)
                                                  .BuildNew();
            Db.Insert(history);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_delete_orphaned_items_by_album()
        {
            GivenArtist();

            var history = Builder<History.History>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.ArtistId = _artist.Id)
                                                  .BuildNew();
            Db.Insert(history);

            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_unorphaned_data_by_artist()
        {
            GivenArtist();
            GivenAlbum();

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .All()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.AlbumId = _album.Id)
                                                  .TheFirst(1)
                                                  .With(h => h.ArtistId = _artist.Id)
                                                  .BuildListOfNew();

            Db.InsertMany(history);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.ArtistId == _artist.Id);
        }

        [Test]
        public void should_not_delete_unorphaned_data_by_album()
        {
            GivenArtist();
            GivenAlbum();

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .All()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.ArtistId = _artist.Id)
                                                  .TheFirst(1)
                                                  .With(h => h.AlbumId = _album.Id)
                                                  .BuildListOfNew();

            Db.InsertMany(history);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.AlbumId == _album.Id);
        }
    }
}
