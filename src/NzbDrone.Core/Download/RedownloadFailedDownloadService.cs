using NLog;
using NzbDrone.Core.Books;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public class RedownloadFailedDownloadService : IHandle<DownloadFailedEvent>
    {
        private readonly IConfigService _configService;
        private readonly IBookService _bookService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public RedownloadFailedDownloadService(IConfigService configService,
                                               IBookService bookService,
                                               IManageCommandQueue commandQueueManager,
                                               Logger logger)
        {
            _configService = configService;
            _bookService = bookService;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        [EventHandleOrder(EventHandleOrder.Last)]
        public void Handle(DownloadFailedEvent message)
        {
            if (message.SkipReDownload)
            {
                _logger.Debug("Skip redownloading requested by user");
                return;
            }

            if (!_configService.AutoRedownloadFailed)
            {
                _logger.Debug("Auto redownloading failed books is disabled");
                return;
            }

            if (message.BookIds.Count == 1)
            {
                _logger.Debug("Failed download only contains one book, searching again");

                _commandQueueManager.Push(new BookSearchCommand(message.BookIds));

                return;
            }

            var albumsInArtist = _bookService.GetBooksByAuthor(message.AuthorId);

            if (message.BookIds.Count == albumsInArtist.Count)
            {
                _logger.Debug("Failed download was entire author, searching again");

                _commandQueueManager.Push(new AuthorSearchCommand
                {
                    AuthorId = message.AuthorId
                });

                return;
            }

            _logger.Debug("Failed download contains multiple books, searching again");

            _commandQueueManager.Push(new BookSearchCommand(message.BookIds));
        }
    }
}
