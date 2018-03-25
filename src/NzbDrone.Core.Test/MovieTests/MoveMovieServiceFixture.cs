using System.IO;
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

            Mocker.GetMock<IMovieService>()
                  .Setup(s => s.GetMovie(It.IsAny<int>()))
                  .Returns(_movie);
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

            Assert.Throws<IOException>(() => Subject.Execute(_command));

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_no_update_movie_path_on_error()
        {
            GivenFailedMove();

            Assert.Throws<IOException>(() => Subject.Execute(_command));

            ExceptionVerification.ExpectedErrors(1);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.IsAny<Movie>()), Times.Never());
        }

        [Test]
        public void should_build_new_path_when_root_folder_is_provided()
        {
            _command.DestinationPath = null;
            _command.DestinationRootFolder = @"C:\Test\Movie3".AsOsAgnostic();

            var expectedPath = @"C:\Test\Movie3\Movie".AsOsAgnostic();

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetMovieFolder(It.IsAny<Movie>(), null))
                  .Returns("Movie");

            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.Is<Movie>(s => s.Path == expectedPath)), Times.Once());
        }

        [Test]
        public void should_use_destination_path_if_destination_root_folder_is_blank()
        {
            Subject.Execute(_command);

            Mocker.GetMock<IMovieService>()
                  .Verify(v => v.UpdateMovie(It.Is<Movie>(s => s.Path == _command.DestinationPath)), Times.Once());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetMovieFolder(It.IsAny<Movie>(), null), Times.Never());
        }
    }
}
