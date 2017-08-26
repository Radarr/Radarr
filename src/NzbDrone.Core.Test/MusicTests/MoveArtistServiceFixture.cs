using System.IO;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class MoveArtistServiceFixture : CoreTest<MoveArtistService>
    {
        private Artist _artist;
        private MoveArtistCommand _command;

        [SetUp]
        public void Setup()
        {
            _artist = Builder<Artist>
                .CreateNew()
                .Build();

            _command = new MoveArtistCommand
            {
                           ArtistId = 1,
                           SourcePath = @"C:\Test\Music\Artist".AsOsAgnostic(),
                           DestinationPath = @"C:\Test\Music2\Artist".AsOsAgnostic()
                       };

            Mocker.GetMock<IArtistService>()
                  .Setup(s => s.GetArtist(It.IsAny<int>()))
                  .Returns(_artist);
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
        public void should_no_update_artist_path_on_error()
        {
            GivenFailedMove();

            Assert.Throws<IOException>(() => Subject.Execute(_command));

            ExceptionVerification.ExpectedErrors(1);

            Mocker.GetMock<IArtistService>()
                  .Verify(v => v.UpdateArtist(It.IsAny<Artist>()), Times.Never());
        }

        [Test]
        public void should_build_new_path_when_root_folder_is_provided()
        {
            _command.DestinationPath = null;
            _command.DestinationRootFolder = @"C:\Test\Music3".AsOsAgnostic();
            
            var expectedPath = @"C:\Test\Music3\Artist".AsOsAgnostic();

            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetArtistFolder(It.IsAny<Artist>(), null))
                  .Returns("Artist");

            Subject.Execute(_command);

            Mocker.GetMock<IArtistService>()
                  .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.Path == expectedPath)), Times.Once());
        }

        [Test]
        public void should_use_destination_path_if_destination_root_folder_is_blank()
        {
            Subject.Execute(_command);

            Mocker.GetMock<IArtistService>()
                  .Verify(v => v.UpdateArtist(It.Is<Artist>(s => s.Path == _command.DestinationPath)), Times.Once());

            Mocker.GetMock<IBuildFileNames>()
                  .Verify(v => v.GetArtistFolder(It.IsAny<Artist>(), null), Times.Never());
        }
    }
}
