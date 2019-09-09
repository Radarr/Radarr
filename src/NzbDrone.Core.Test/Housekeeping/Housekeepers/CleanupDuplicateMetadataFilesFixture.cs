using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using NzbDrone.Core.Extras.Metadata;
using NzbDrone.Core.Extras.Metadata.Files;
using NzbDrone.Core.Housekeeping.Housekeepers;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Housekeeping.Housekeepers
{
    [TestFixture]
    public class CleanupDuplicateMetadataFilesFixture : DbTest<CleanupDuplicateMetadataFiles, MetadataFile>
    {
        [Test]
        public void should_not_delete_metadata_files_when_they_are_for_the_same_artist_but_different_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.ArtistMetadata)
                                             .With(m => m.ArtistId = 1)
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_not_delete_metadata_files_for_different_artist()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.ArtistMetadata)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_delete_metadata_files_when_they_are_for_the_same_artist_and_consumer()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.ArtistMetadata)
                                             .With(m => m.ArtistId = 1)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(1);
        }

        [Test]
        public void should_not_delete_metadata_files_when_there_is_only_one_for_that_artist_and_consumer()
        {
            var file = Builder<MetadataFile>.CreateNew()
                                         .BuildNew();

            Db.Insert(file);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(1);
        }

        [Test]
        public void should_not_delete_metadata_files_when_they_are_for_the_same_album_but_different_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.AlbumMetadata)
                                             .With(m => m.ArtistId = 1)
                                             .With(m => m.AlbumId = 1)
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_not_delete_metadata_files_for_different_album()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.AlbumMetadata)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .With(m => m.ArtistId = 1)
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_delete_metadata_files_when_they_are_for_the_same_album_and_consumer()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.AlbumMetadata)
                                             .With(m => m.ArtistId = 1)
                                             .With(m => m.AlbumId = 1)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(1);
        }

        [Test]
        public void should_not_delete_metadata_files_when_there_is_only_one_for_that_album_and_consumer()
        {
            var file = Builder<MetadataFile>.CreateNew()
                                         .BuildNew();

            Db.Insert(file);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(1);
        }

        [Test]
        public void should_not_delete_metadata_files_when_they_are_for_the_same_track_but_different_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.TrackMetadata)
                                             .With(m => m.TrackFileId = 1)
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_not_delete_metadata_files_for_different_track()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.TrackMetadata)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_delete_metadata_files_when_they_are_for_the_same_track_and_consumer()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.TrackMetadata)
                                             .With(m => m.TrackFileId = 1)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(1);
        }

        [Test]
        public void should_not_delete_metadata_files_when_there_is_only_one_for_that_track_and_consumer()
        {
            var file = Builder<MetadataFile>.CreateNew()
                                            .BuildNew();

            Db.Insert(file);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(1);
        }
    }
}
