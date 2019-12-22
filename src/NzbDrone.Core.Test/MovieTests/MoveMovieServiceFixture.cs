using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Commands;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MovieTests
{
    [TestFixture]
    public class MoveMovieServiceFixture : CoreTest<MoveMovieService>
    {
        private Movie _movie;
        private MoveMovieCommand _command;
        private BulkMoveMovieCommand _bulkCommand;

        [SetUp]
        public void Setup()
        {
            _movie = Builder<Movie>
                .CreateNew()
                .Build();

            _command = new MoveMovieCommand
            {
                MovieId = 1,
                SourcePath = @"C:\Test\Movies\Movie".AsOsAgnostic(),
                DestinationPath = @"C:\Test\Movies2\Movie".AsOsAgnostic()
            };

            _bulkCommand = new BulkMoveMovieCommand
            {
                Movies = new List<BulkMoveMovie>
                                    {
                                        new BulkMoveMovie
                                        {
                                            MovieId = 1,
                                            SourcePath = @"C:\Test\Movies\Movie".AsOsAgnostic()
                                        }
                                    },
                DestinationRootFolder = @"C:\Test\Movies2".AsOsAgnostic()
            };

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_movie);

            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(true);
        }

        private void GivenFailedMove()
        {
            Mocker.GetMock<IDiskTransferService>()
                  .Setup(s => s.TransferFolder(It.IsAny<string>(), It.IsAny<string>(), TransferMode.Move, true))
                  .Throws<IOException>();
        }

        [Test]
        public void should_log_error_when_move_throws_an_exception()
        {
            GivenFailedMove();

            Subject.Execute(_command);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_revert_movie_path_on_error()
        {
            GivenFailedMove();

            Subject.Execute(_command);

            ExceptionVerification.ExpectedErrors(1);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.IsAny<Movie>()), Times.Once());
        }

        [Test]
        public void should_use_destination_path()
        {
            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move, It.IsAny<bool>()), Times.Once());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetMovieFolder(It.IsAny<Movie>(), null), Times.Never());
        }

        [Test]
        public void should_build_new_path_when_root_folder_is_provided()
        {
            var movieFolder = "Movie";
            var expectedPath = Path.Combine(_bulkCommand.DestinationRootFolder, movieFolder);

            Mocker.GetMock<IBuildFileNames>()
                    .Setup(s => s.GetMovieFolder(It.IsAny<Movie>(), null))
                    .Returns(movieFolder);

            Subject.Execute(_bulkCommand);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_bulkCommand.Movies.First().SourcePath, expectedPath, TransferMode.Move, It.IsAny<bool>()), Times.Once());
        }

        [Test]
        public void should_skip_movie_folder_if_it_does_not_exist()
        {
            Mocker.GetMock<IDiskProvider>()
                  .Setup(s => s.FolderExists(It.IsAny<string>()))
                  .Returns(false);


            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                  .Verify(v => v.TransferFolder(_command.SourcePath, _command.DestinationPath, TransferMode.Move, It.IsAny<bool>()), Times.Never());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetMovieFolder(It.IsAny<Movie>(), null), Times.Never());
        }
    }
}
