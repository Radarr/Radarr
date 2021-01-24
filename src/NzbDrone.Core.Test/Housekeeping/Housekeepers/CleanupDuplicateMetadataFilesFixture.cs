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
        public void should_not_delete_metadata_files_when_they_are_for_the_same_author_but_different_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.AuthorMetadata)
                                             .With(m => m.AuthorId = 1)
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_not_delete_metadata_files_for_different_author()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.AuthorMetadata)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_delete_metadata_files_when_they_are_for_the_same_author_and_consumer()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.AuthorMetadata)
                                             .With(m => m.AuthorId = 1)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(1);
        }

        [Test]
        public void should_not_delete_metadata_files_when_there_is_only_one_for_that_author_and_consumer()
        {
            var file = Builder<MetadataFile>.CreateNew()
                                         .BuildNew();

            Db.Insert(file);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(1);
        }

        [Test]
        public void should_not_delete_metadata_files_when_they_are_for_the_same_book_but_different_consumers()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.BookMetadata)
                                             .With(m => m.AuthorId = 1)
                                             .With(m => m.BookId = 1)
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_not_delete_metadata_files_for_different_book()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.BookMetadata)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .With(m => m.AuthorId = 1)
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(files.Count);
        }

        [Test]
        public void should_delete_metadata_files_when_they_are_for_the_same_book_and_consumer()
        {
            var files = Builder<MetadataFile>.CreateListOfSize(2)
                                             .All()
                                             .With(m => m.Type = MetadataType.BookMetadata)
                                             .With(m => m.AuthorId = 1)
                                             .With(m => m.BookId = 1)
                                             .With(m => m.Consumer = "XbmcMetadata")
                                             .BuildListOfNew();

            Db.InsertMany(files);
            Subject.Clean();
            AllStoredModels.Count.Should().Be(1);
        }

        [Test]
        public void should_not_delete_metadata_files_when_there_is_only_one_for_that_book_and_consumer()
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
                                             .With(m => m.Type = MetadataType.BookMetadata)
                                             .With(m => m.BookFileId = 1)
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
                                             .With(m => m.Type = MetadataType.BookMetadata)
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
                                             .With(m => m.Type = MetadataType.BookMetadata)
                                             .With(m => m.BookFileId = 1)
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
