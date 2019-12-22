using System.Collections.Generic;
using NLog;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.IndexerSearch;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Download
{
    public class RedownloadFailedDownloadService : IHandleAsync<DownloadFailedEvent>
    {
        private readonly IConfigService _configService;
        private readonly IMovieService _movieService;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public RedownloadFailedDownloadService(IConfigService configService,
                                               IMovieService movieService,
                                               IManageCommandQueue commandQueueManager,
                                               Logger logger)
        {
            _configService = configService;
            _movieService = movieService;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        public void HandleAsync(DownloadFailedEvent message)
        {
            if (!_configService.AutoRedownloadFailed)
            {
                _logger.Debug("Auto redownloading failed movies is disabled");
                return;
            }

            if (message.MovieId != 0)
            {
                _logger.Debug("Failed download contains a movie, searching again.");
                _commandQueueManager.Push(new MoviesSearchCommand { MovieIds = new List<int> { message.MovieId } });
            }
        }
    }
}
