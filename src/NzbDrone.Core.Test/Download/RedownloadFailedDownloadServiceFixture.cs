using System.Collections.Generic;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.Download
{
    [TestFixture]
    public class RedownloadFailedDownloadServiceFixture : CoreTest<RedownloadFailedDownloadService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IConfigService>()
                .Setup(x => x.AutoRedownloadFailed)
                .Returns(true);

            Mocker.GetMock<IBookService>()
                .Setup(x => x.GetBooksByAuthor(It.IsAny<int>()))
                .Returns(Builder<Book>.CreateListOfSize(3).Build() as List<Book>);
        }

        [Test]
        public void should_skip_redownload_if_event_has_skipredownload_set()
        {
            var failedEvent = new DownloadFailedEvent
            {
                AuthorId = 1,
                BookIds = new List<int> { 1 },
                SkipReDownload = true
            };

            Subject.Handle(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<Command>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }

        [Test]
        public void should_skip_redownload_if_redownload_failed_disabled()
        {
            var failedEvent = new DownloadFailedEvent
            {
                AuthorId = 1,
                BookIds = new List<int> { 1 }
            };

            Mocker.GetMock<IConfigService>()
                .Setup(x => x.AutoRedownloadFailed)
                .Returns(false);

            Subject.Handle(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<Command>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }

        [Test]
        public void should_redownload_album_on_failure()
        {
            var failedEvent = new DownloadFailedEvent
            {
                AuthorId = 1,
                BookIds = new List<int> { 2 }
            };

            Subject.Handle(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.Is<BookSearchCommand>(c => c.BookIds.Count == 1 &&
                                                              c.BookIds[0] == 2),
                                    It.IsAny<CommandPriority>(),
                                    It.IsAny<CommandTrigger>()),
                        Times.Once());

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<AuthorSearchCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }

        [Test]
        public void should_redownload_multiple_albums_on_failure()
        {
            var failedEvent = new DownloadFailedEvent
            {
                AuthorId = 1,
                BookIds = new List<int> { 2, 3 }
            };

            Subject.Handle(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.Is<BookSearchCommand>(c => c.BookIds.Count == 2 &&
                                                              c.BookIds[0] == 2 &&
                                                              c.BookIds[1] == 3),
                                    It.IsAny<CommandPriority>(),
                                    It.IsAny<CommandTrigger>()),
                        Times.Once());

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<AuthorSearchCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }

        [Test]
        public void should_redownload_artist_on_failure()
        {
            // note that artist is set to have 3 albums in setup
            var failedEvent = new DownloadFailedEvent
            {
                AuthorId = 2,
                BookIds = new List<int> { 1, 2, 3 }
            };

            Subject.Handle(failedEvent);

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.Is<AuthorSearchCommand>(c => c.AuthorId == failedEvent.AuthorId),
                                    It.IsAny<CommandPriority>(),
                                    It.IsAny<CommandTrigger>()),
                        Times.Once());

            Mocker.GetMock<IManageCommandQueue>()
                .Verify(x => x.Push(It.IsAny<BookSearchCommand>(), It.IsAny<CommandPriority>(), It.IsAny<CommandTrigger>()),
                        Times.Never());
        }
    }
}
