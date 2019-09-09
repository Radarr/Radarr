using System.Linq;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Download
{
    public class RedownloadFailedDownloadService : IHandleAsync<DownloadFailedEvent>
    {
        private readonly IConfigService _configService;
        private readonly IAlbumService _albumService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public RedownloadFailedDownloadService(IConfigService configService,
                                               IAlbumService albumService,
                                               IManageCommandQueue commandQueueManager,
                                               Logger logger)
        {
            _configService = configService;
            _albumService = albumService;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        public void HandleAsync(DownloadFailedEvent message)
        {
            if (message.SkipReDownload)
            {
                _logger.Debug("Skip redownloading requested by user");
                return;
            }

            if (!_configService.AutoRedownloadFailed)
            {
                _logger.Debug("Auto redownloading failed albums is disabled");
                return;
            }

            if (message.AlbumIds.Count == 1)
            {
                _logger.Debug("Failed download only contains one album, searching again");

                _commandQueueManager.Push(new AlbumSearchCommand(message.AlbumIds));

                return;
            }

            var albumsInArtist = _albumService.GetAlbumsByArtist(message.ArtistId);

            if (message.AlbumIds.Count == albumsInArtist.Count)
            {
                _logger.Debug("Failed download was entire artist, searching again");

                _commandQueueManager.Push(new ArtistSearchCommand
                {
                    ArtistId = message.ArtistId
                });

                return;
            }

            _logger.Debug("Failed download contains multiple albums, searching again");

            _commandQueueManager.Push(new AlbumSearchCommand(message.AlbumIds));
        }
    }
}
