using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MediaFiles.EpisodeFileMovingServiceTests
{
    [TestFixture]
    public class MoveEpisodeFileFixture : CoreTest<MovieFileMovingService>
    {
        private Movie _series;
        private MovieFile _episodeFile;
        private LocalMovie _localEpisode;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Movie>.CreateNew()
                                     .With(s => s.Path = @"C:\Test\TV\Series".AsOsAgnostic())
                                     .Build();

            _episodeFile = Builder<MovieFile>.CreateNew()
                                               .With(f => f.Path = null)
                                               .With(f => f.RelativePath = @"Season 1\File.avi")
                                               .Build();

            _localEpisode = Builder<LocalMovie>.CreateNew()
                                                 .With(l => l.Movie = _series)
                                                 .Build();

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildFileName(It.IsAny<Movie>(), It.IsAny<MovieFile>(), null))
                  .Returns("File Name");

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.BuildFilePath(It.IsAny<Movie>(), It.IsAny<string>(), It.IsAny<string>()))
                  .Returns(@"C:\Test\TV\Series\File Name.avi".AsOsAgnostic());

            var rootFolder = @"C:\Test\TV\".AsOsAgnostic();
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

            Subject.MoveMovieFile(_episodeFile, _localEpisode);
        }

        [Test]
        public void should_catch_InvalidOperationException_during_folder_inheritance()
        {
            WindowsOnly();

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.InheritFolderPermissions(It.IsAny<string>()))
                  .Throws<InvalidOperationException>();

            Subject.MoveMovieFile(_episodeFile, _localEpisode);
        }

        [Test]
        public void should_notify_on_series_folder_creation()
        {
            Subject.MoveMovieFile(_episodeFile, _localEpisode);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<MovieFolderCreatedEvent>(It.Is<MovieFolderCreatedEvent>(p =>
                      p.MovieFolder.IsNotNullOrWhiteSpace())), Times.Once());
        }

        [Test]
        public void should_not_notify_if_series_folder_already_exists()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(_series.Path))
                  .Returns(true);

            Subject.MoveMovieFile(_episodeFile, _localEpisode);

            Mocker.GetMock<IEventAggregator>()
                  .Verify(s => s.PublishEvent<MovieFolderCreatedEvent>(It.Is<MovieFolderCreatedEvent>(p =>
                      p.SeriesFolder.IsNotNullOrWhiteSpace())), Times.Never());
        }
    }
}
