using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Music;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedHistoryItemsFixture : DbTest<CleanupOrphanedHistoryItems, History.History>
    {
        private Author _artist;
        private Book _album;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Author>.CreateNew()
                                     .BuildNew();

            _album = Builder<Book>.CreateNew()
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
                                                  .With(h => h.BookId = _album.Id)
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
                                                  .With(h => h.AuthorId = _artist.Id)
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
                                                  .With(h => h.BookId = _album.Id)
                                                  .TheFirst(1)
                                                  .With(h => h.AuthorId = _artist.Id)
                                                  .BuildListOfNew();

            Db.InsertMany(history);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.AuthorId == _artist.Id);
        }

        [Test]
        public void should_not_delete_unorphaned_data_by_album()
        {
            GivenArtist();
            GivenAlbum();

            var history = Builder<History.History>.CreateListOfSize(2)
                                                  .All()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .With(h => h.AuthorId = _artist.Id)
                                                  .TheFirst(1)
                                                  .With(h => h.BookId = _album.Id)
                                                  .BuildListOfNew();

            Db.InsertMany(history);

            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
            AllStoredModels.Should().Contain(h => h.BookId == _album.Id);
        }
    }
}
