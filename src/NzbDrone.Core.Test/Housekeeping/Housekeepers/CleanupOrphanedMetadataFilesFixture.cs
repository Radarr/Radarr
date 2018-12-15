using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupOrphanedMetadataFilesFixture : DbTest<CleanupOrphanedMetadataFiles, MetadataFile>
    {
        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_artist()
        {
            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.TrackFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_album()
        {
            var artist = Builder<Artist>.CreateNew()
                                        .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.ArtistId = artist.Id)
                                                    .With(m => m.TrackFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_artist()
        {
            var artist = Builder<Artist>.CreateNew()
                                        .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.ArtistId = artist.Id)
                                                    .With(m => m.AlbumId = null)
                                                    .With(m => m.TrackFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            var countMods = AllStoredModels.Count;
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_album()
        {
            var artist = Builder<Artist>.CreateNew()
                                        .BuildNew();

            var album = Builder<Album>.CreateNew()
                .BuildNew();

            Db.Insert(artist);
            Db.Insert(album);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.ArtistId = artist.Id)
                                                    .With(m => m.AlbumId = album.Id)
                                                    .With(m => m.TrackFileId = null)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_metadata_files_that_dont_have_a_coresponding_track_file()
        {
            var artist = Builder<Artist>.CreateNew()
                                        .BuildNew();

            var album = Builder<Album>.CreateNew()
                .BuildNew();

            Db.Insert(artist);
            Db.Insert(album);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.ArtistId = artist.Id)
                                                    .With(m => m.AlbumId = album.Id)
                                                    .With(m => m.TrackFileId = 10)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().BeEmpty();
        }

        [Test]
        public void should_not_delete_metadata_files_that_have_a_coresponding_track_file()
        {
            var artist = Builder<Artist>.CreateNew()
                                        .BuildNew();

            var album = Builder<Album>.CreateNew()
                                        .BuildNew();

            var trackFile = Builder<TrackFile>.CreateNew()
                                                  .With(h => h.Quality = new QualityModel())
                                                  .BuildNew();

            Db.Insert(artist);
            Db.Insert(album);
            Db.Insert(trackFile);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                    .With(m => m.ArtistId = artist.Id)
                                                    .With(m => m.AlbumId = album.Id)
                                                    .With(m => m.TrackFileId = trackFile.Id)
                                                    .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(1);
        }

        [Test]
        public void should_delete_album_metadata_files_that_have_albumid_of_zero()
        {
            var artist = Builder<Artist>.CreateNew()
                .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                .With(m => m.ArtistId = artist.Id)
                .With(m => m.Type = MetadataType.AlbumMetadata)
                .With(m => m.AlbumId = 0)
                .With(m => m.TrackFileId = null)
                .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }

        [Test]
        public void should_delete_album_image_files_that_have_albumid_of_zero()
        {
            var artist = Builder<Artist>.CreateNew()
                .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                .With(m => m.ArtistId = artist.Id)
                .With(m => m.Type = MetadataType.AlbumImage)
                .With(m => m.AlbumId = 0)
                .With(m => m.TrackFileId = null)
                .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }

        [Test]
        public void should_delete_track_metadata_files_that_have_trackfileid_of_zero()
        {
            var artist = Builder<Artist>.CreateNew()
                                        .BuildNew();

            Db.Insert(artist);

            var metadataFile = Builder<MetadataFile>.CreateNew()
                                                 .With(m => m.ArtistId = artist.Id)
                                                 .With(m => m.Type = MetadataType.TrackMetadata)
                                                 .With(m => m.TrackFileId = 0)
                                                 .BuildNew();

            Db.Insert(metadataFile);
            Subject.Clean();
            AllStoredModels.Should().HaveCount(0);
        }
    }
}
