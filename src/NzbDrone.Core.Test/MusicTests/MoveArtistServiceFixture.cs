using System.Collections.Generic;
using System.IO;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Organizer;
using NzbDrone.Core.Test.Framework;
using NzbDrone.Test.Common;

namespace NzbDrone.Core.Test.MusicTests
{
    [TestFixture]
    public class MoveAuthorServiceFixture : CoreTest<MoveAuthorService>
    {
        private Author _author;
        private MoveAuthorCommand _command;
        private BulkMoveAuthorCommand _bulkCommand;

        [SetUp]
        public void Setup()
        {
            _author = Builder<Author>
                .CreateNew()
                .Build();

            _command = new MoveAuthorCommand
            {
                AuthorId = 1,
                SourcePath = @"C:\Test\Music\Author".AsOsAgnostic(),
                DestinationPath = @"C:\Test\Music2\Author".AsOsAgnostic()
            };

            _bulkCommand = new BulkMoveAuthorCommand
            {
                Author = new List<BulkMoveAuthor>
                {
                    new BulkMoveAuthor
                    {
                        AuthorId = 1,
                        SourcePath = @"C:\Test\Music\Author".AsOsAgnostic()
                    }
                },
                DestinationRootFolder = @"C:\Test\Music2".AsOsAgnostic()
            };

            Mocker.GetMock<IAuthorService>()
                .Setup(s => s.GetAuthor(It.IsAny<int>()))
                .Returns(_author);

            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FolderExists(It.IsAny<string>()))
                .Returns(true);
        }

        private void GivenFailedMove()
        {
            Mocker.GetMock<IDiskTransferService>()
                .Setup(s => s.TransferFolder(It.IsAny<string>(), It.IsAny<string>(), TransferMode.Move))
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
        public void should_revert_author_path_on_error()
        {
            GivenFailedMove();

            Subject.Execute(_command);

            ExceptionVerification.ExpectedErrors(1);

            Mocker.GetMock<IAuthorService>()
                .Verify(v => v.UpdateAuthor(It.IsAny<Author>()), Times.Once());
        }

        [Test]
        public void should_use_destination_path()
        {
            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                .Verify(
                    v => v.TransferFolder(_command.SourcePath,
                                          _command.DestinationPath,
                                          TransferMode.Move),
                    Times.Once());

            Mocker.GetMock<IBuildFileNames>()
                .Verify(v => v.GetAuthorFolder(It.IsAny<Author>(), null), Times.Never());
        }

        [Test]
        public void should_build_new_path_when_root_folder_is_provided()
        {
            var authorFolder = "Author";
            var expectedPath = Path.Combine(_bulkCommand.DestinationRootFolder, authorFolder);

            Mocker.GetMock<IBuildFileNames>()
                .Setup(s => s.GetAuthorFolder(It.IsAny<Author>(), null))
                .Returns(authorFolder);

            Subject.Execute(_bulkCommand);

            Mocker.GetMock<IDiskTransferService>()
                .Verify(
                    v => v.TransferFolder(_bulkCommand.Author.First().SourcePath,
                                          expectedPath,
                                          TransferMode.Move),
                    Times.Once());
        }

        [Test]
        public void should_skip_author_folder_if_it_does_not_exist()
        {
            Mocker.GetMock<IDiskProvider>()
                .Setup(s => s.FolderExists(It.IsAny<string>()))
                .Returns(false);

            Subject.Execute(_command);

            Mocker.GetMock<IDiskTransferService>()
                .Verify(
                    v => v.TransferFolder(_command.SourcePath,
                        _command.DestinationPath,
                        TransferMode.Move), Times.Never());

            Mocker.GetMock<IBuildFileNames>()
                .Verify(v => v.GetAuthorFolder(It.IsAny<Author>(), null), Times.Never());
        }
    }
}
