using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    public class MovieFolderServiceFixture : CoreTest<MovieFolderService>
    {
        private Movie _movie;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>.CreateNew()
                                    .With(m => m.Path = "C:\\Movies\\Wrong Name".AsOsAgnostic())
                                    .Build();
        }

        private NamingConfig GivenNamingConfig(bool renameMovies)
        {
            return new NamingConfig { RenameMovies = renameMovies };
        }

        private void GivenExpectedMovieFolder(string expectedPath)
        {
            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>(), It.IsAny<List<RootFolder>>()))
                  .Returns("C:\\Movies".AsOsAgnostic());

            Mocker.GetMock<IBuildMoviePaths>()
                  .Setup(s => s.BuildPath(_movie, false))
                  .Returns(expectedPath);
        }

        [Test]
        public void should_return_null_when_rename_movies_disabled()
        {
            var namingConfig = GivenNamingConfig(false);

            var result = Subject.GetExpectedMovieFolder(_movie, namingConfig);

            result.Should().BeNull();

            Mocker.GetMock<IBuildMoviePaths>()
                  .Verify(v => v.BuildPath(It.IsAny<Movie>(), It.IsAny<bool>()), Times.Never());
        }

        [Test]
        public void should_return_expected_folder_when_rename_movies_enabled()
        {
            var expectedPath = "C:\\Movies\\Correct Name (2020)".AsOsAgnostic();
            var namingConfig = GivenNamingConfig(true);
            GivenExpectedMovieFolder(expectedPath);

            var result = Subject.GetExpectedMovieFolder(_movie, namingConfig);

            result.Should().Be(expectedPath);
        }

        [Test]
        public void should_not_move_folder_when_source_equals_destination()
        {
            var result = Subject.TryMoveMovieFolder(_movie, _movie.Path, _movie.Path);

            result.Should().BeTrue();

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransferMode>()), Times.Never());
        }

        [Test]
        public void should_move_folder_update_path_and_publish_event_on_success()
        {
            var sourcePath = _movie.Path;
            var destinationPath = "C:\\Movies\\Correct Name (2020)".AsOsAgnostic();

            var result = Subject.TryMoveMovieFolder(_movie, sourcePath, destinationPath);

            result.Should().BeTrue();
            _movie.Path.Should().Be(destinationPath);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(sourcePath, destinationPath, TransferMode.Move), Times.Once());

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(_movie), Times.Once());

            Mocker.GetMock<IEventAggregator>()
                  .Verify(v => v.PublishEvent(It.IsAny<MovieMovedEvent>()), Times.Once());
        }

        [Test]
        public void should_log_error_and_return_false_when_transfer_throws()
        {
            var sourcePath = _movie.Path;
            var destinationPath = "C:\\Movies\\Correct Name (2020)".AsOsAgnostic();

            Mocker.GetMock<IDiskTransferService>()
                  .Setup(s => s.TransferFolder(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TransferMode>()))
                  .Throws(new IOException("Access denied"));

            var result = Subject.TryMoveMovieFolder(_movie, sourcePath, destinationPath);

            result.Should().BeFalse();
            _movie.Path.Should().Be(sourcePath);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.IsAny<Movie>()), Times.Never());

            ExceptionVerification.WaitForErrors(1, 500);
        }
    }
}
