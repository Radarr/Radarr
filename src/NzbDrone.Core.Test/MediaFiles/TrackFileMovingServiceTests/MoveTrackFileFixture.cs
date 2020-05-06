using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.TrackFileMovingServiceTests
{
    [TestFixture]
    public class MoveTrackFileFixture : CoreTest<TrackFileMovingService>
    {
        private Author _artist;
        private BookFile _trackFile;
        private LocalTrack _localtrack;

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

            _localtrack = Builder<LocalTrack>.CreateNew()
                                                 .With(l => l.Artist = _artist)
                                                 .With(l => l.Album = Builder<Book>.CreateNew().Build())
                                                 .Build();

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildTrackFileName(It.IsAny<Author>(), It.IsAny<Book>(), It.IsAny<BookFile>(), null, null))
                  .Returns("File Name");

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildTrackFilePath(It.IsAny<Author>(), It.IsAny<Book>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(@"C:\Test\Music\Artist\Album\File Name.mp3".AsOsAgnostic());

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildAlbumPath(It.IsAny<Author>(), It.IsAny<Book>()))
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

            Subject.MoveTrackFile(_trackFile, _localtrack);
        }

        [Test]
        public void should_catch_InvalidOperationException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<string>()))
                  .Throws<InvalidOperationException>();

            Subject.MoveTrackFile(_trackFile, _localtrack);
        }

        [Test]
        public void should_notify_on_artist_folder_creation()
        {
            Subject.MoveTrackFile(_trackFile, _localtrack);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<TrackFolderCreatedEvent>(It.Is<TrackFolderCreatedEvent>(p =>
                      p.ArtistFolder.IsNotNullOrWhiteSpace())), Times.Once());
        }

        [Test]
        public void should_notify_on_album_folder_creation()
        {
            Subject.MoveTrackFile(_trackFile, _localtrack);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<TrackFolderCreatedEvent>(It.Is<TrackFolderCreatedEvent>(p =>
                      p.AlbumFolder.IsNotNullOrWhiteSpace())), Times.Once());
        }

        [Test]
        public void should_not_notify_if_artist_folder_already_exists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_artist.Path))
                  .Returns(true);

            Subject.MoveTrackFile(_trackFile, _localtrack);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<TrackFolderCreatedEvent>(It.Is<TrackFolderCreatedEvent>(p =>
                      p.ArtistFolder.IsNotNullOrWhiteSpace())), Times.Never());
        }
    }
}
