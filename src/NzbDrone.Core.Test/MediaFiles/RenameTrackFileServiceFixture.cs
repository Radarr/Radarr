using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.MediaFiles
{
    public class RenameTrackFileServiceFixture : CoreTest<RenameBookFileService>
    {
        private Author _artist;
        private List<BookFile> _trackFiles;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Author>.CreateNew()
                                     .Build();

            _trackFiles = Builder<BookFile>.CreateListOfSize(2)
                                                .All()
                                                .With(e => e.Author = _artist)
                                                .Build()
                                                .ToList();

            Mocker.GetMock<IAuthorService>()
                  .Setup(s => s.GetAuthor(_artist.Id))
                  .Returns(_artist);
        }

        private void GivenNoTrackFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(new List<BookFile>());
        }

        private void GivenTrackFiles()
        {
            Mocker.GetMock<IMediaFileService>()
                  .Setup(s => s.Get(It.IsAny<IEnumerable<int>>()))
                  .Returns(_trackFiles);
        }

        private void GivenMovedFiles()
        {
            Mocker.GetMock<IMoveBookFiles>()
                  .Setup(s => s.MoveBookFile(It.IsAny<BookFile>(), _artist));
        }

        [Test]
        public void should_not_publish_event_if_no_files_to_rename()
        {
            GivenNoTrackFiles();

            Subject.Execute(new RenameFilesCommand(_artist.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<AuthorRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_not_publish_event_if_no_files_are_renamed()
        {
            GivenTrackFiles();

            Mocker.GetMock<IMoveBookFiles>()
                  .Setup(s => s.MoveBookFile(It.IsAny<BookFile>(), It.IsAny<Author>()))
                  .Throws(new SameFilenameException("Same file name", "Filename"));

            Subject.Execute(new RenameFilesCommand(_artist.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<AuthorRenamedEvent>()), Times.Never());
        }

        [Test]
        public void should_publish_event_if_files_are_renamed()
        {
            GivenTrackFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_artist.Id, new List<int> { 1 }));

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<AuthorRenamedEvent>()), Times.Once());
        }

        [Test]
        public void should_update_moved_files()
        {
            GivenTrackFiles();
            GivenMovedFiles();

            Subject.Execute(new RenameFilesCommand(_artist.Id, new List<int> { 1 }));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Update(It.IsAny<BookFile>()), Times.Exactly(2));
        }

        [Test]
        public void should_get_trackfiles_by_ids_only()
        {
            GivenTrackFiles();
            GivenMovedFiles();

            var files = new List<int> { 1 };

            Subject.Execute(new RenameFilesCommand(_artist.Id, files));

            Mocker.GetMock<IMediaFileService>()
                  .Verify(v => v.Get(files), Times.Once());
        }
    }
}
