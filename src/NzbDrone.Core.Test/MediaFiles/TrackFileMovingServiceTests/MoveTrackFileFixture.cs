using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.TrackFileMovingServiceTests
{
    [TestFixture]
    public class MoveTrackFileFixture : CoreTest<BookFileMovingService>
    {
        private Author _artist;
        private BookFile _trackFile;
        private LocalBook _localtrack;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Author>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\Music\Artist".AsOsAgnostic())
                                     .Build();

            _trackFile = Builder<BookFile>.CreateNew()
                                               .With(f => f.Path = null)
                                               .With(f => f.Path = Path.Combine(_artist.Path, @"Album\File.mp3"))
                                               .Build();

            _localtrack = Builder<LocalBook>.CreateNew()
                                                 .With(l => l.Author = _artist)
                                                 .With(l => l.Book = Builder<Book>.CreateNew().Build())
                                                 .Build();

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildBookFileName(It.IsAny<Author>(), It.IsAny<Book>(), It.IsAny<BookFile>(), null, null))
                  .Returns("File Name");

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildBookFilePath(It.IsAny<Author>(), It.IsAny<Book>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(@"C:\Test\Music\Artist\Album\File Name.mp3".AsOsAgnostic());

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildBookPath(It.IsAny<Author>(), It.IsAny<Book>()))
                  .Returns(@"C:\Test\Music\Artist\Album".AsOsAgnostic());

            var rootFolder = @"C:\Test\Music\".AsOsAgnostic();
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(rootFolder))
                  .Returns(true);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FileExists(It.IsAny<string>()))
                  .Returns(true);
        }

        [Test]
        public void should_catch_UnauthorizedAccessException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<string>()))
                  .Throws<UnauthorizedAccessException>();

            Subject.MoveBookFile(_trackFile, _localtrack);
        }

        [Test]
        public void should_catch_InvalidOperationException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<string>()))
                  .Throws<InvalidOperationException>();

            Subject.MoveBookFile(_trackFile, _localtrack);
        }

        [Test]
        public void should_notify_on_artist_folder_creation()
        {
            Subject.MoveBookFile(_trackFile, _localtrack);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<TrackFolderCreatedEvent>(It.Is<TrackFolderCreatedEvent>(p =>
                      p.AuthorFolder.IsNotNullOrWhiteSpace())), Times.Once());
        }

        [Test]
        public void should_notify_on_album_folder_creation()
        {
            Subject.MoveBookFile(_trackFile, _localtrack);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<TrackFolderCreatedEvent>(It.Is<TrackFolderCreatedEvent>(p =>
                      p.BookFolder.IsNotNullOrWhiteSpace())), Times.Once());
        }

        [Test]
        public void should_not_notify_if_artist_folder_already_exists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_artist.Path))
                  .Returns(true);

            Subject.MoveBookFile(_trackFile, _localtrack);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<TrackFolderCreatedEvent>(It.Is<TrackFolderCreatedEvent>(p =>
                      p.AuthorFolder.IsNotNullOrWhiteSpace())), Times.Never());
        }
    }
}
