using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedMetadataFilesFixture : DbTest<CleanupOrphanedMetadataFiles, MetadataFile>
    {
        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_artist()
        {
            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.BookFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_album()
        {
            var artist = Builder<Author>.CreateNew()
                                        .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.AuthorId = artist.Id)
                                                    .With(m => m.BookFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_artist()
        {
            var artist = Builder<Author>.CreateNew()
                                        .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.AuthorId = artist.Id)
                                                    .With(m => m.BookId = null)
                                                    .With(m => m.BookFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            var countMods = AllStoredModels.Count;
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_album()
        {
            var artist = Builder<Author>.CreateNew()
                                        .BuildNew();

            var album = Builder<Book>.CreateNew()
                .BuildNew();

            Db.Insert(artist);
            Db.Insert(album);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.AuthorId = artist.Id)
                                                    .With(m => m.BookId = album.Id)
                                                    .With(m => m.BookFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_track_file()
        {
            var artist = Builder<Author>.CreateNew()
                                        .BuildNew();

            var album = Builder<Book>.CreateNew()
                .BuildNew();

            Db.Insert(artist);
            Db.Insert(album);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.AuthorId = artist.Id)
                                                    .With(m => m.BookId = album.Id)
                                                    .With(m => m.BookFileId = 10)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_track_file()
        {
            var artist = Builder<Author>.CreateNew()
                                        .BuildNew();

            var album = Builder<Book>.CreateNew()
                                        .BuildNew();

            var trackFile = Builder<BookFile>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .BuildNew();

            Db.Insert(artist);
            Db.Insert(album);
            Db.Insert(trackFile);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.AuthorId = artist.Id)
                                                    .With(m => m.BookId = album.Id)
                                                    .With(m => m.BookFileId = trackFile.Id)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_album_metadata_files_that_have_albumid_of_zero()
        {
            var artist = Builder<Author>.CreateNew()
                .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                .With(m => m.AuthorId = artist.Id)
                .With(m => m.Type = MetadataType.AlbumMetadata)
                .With(m => m.BookId = 0)
                .With(m => m.BookFileId = null)
                .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }

        [Test]
        public void should_delete_album_image_files_that_have_albumid_of_zero()
        {
            var artist = Builder<Author>.CreateNew()
                .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                .With(m => m.AuthorId = artist.Id)
                .With(m => m.Type = MetadataType.AlbumImage)
                .With(m => m.BookId = 0)
                .With(m => m.BookFileId = null)
                .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }

        [Test]
        public void should_delete_track_metadata_files_that_have_trackfileid_of_zero()
        {
            var artist = Builder<Author>.CreateNew()
                                        .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                 .With(m => m.AuthorId = artist.Id)
                                                 .With(m => m.Type = MetadataType.TrackMetadata)
                                                 .With(m => m.BookFileId = 0)
                                                 .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }
    }
}
