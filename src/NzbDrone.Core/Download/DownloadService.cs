using System;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        void DownloadReport(RemoteMovie remoteMovie);
    }

    public class DownloadService : IDownloadService
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IDownloadClientStatusService _downloadClientStatusService;
        private readonly IIndexerFactory _indexerFactory;
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISeedConfigProvider _seedConfigProvider;
        private readonly Logger _logger;

        public DownloadService(IProvideDownloadClient downloadClientProvider,
                               IDownloadClientStatusService downloadClientStatusService,
                               IIndexerFactory indexerFactory,
                               IIndexerStatusService indexerStatusService,
                               IRateLimitService rateLimitService,
                               IEventAggregator eventAggregator,
                               ISeedConfigProvider seedConfigProvider,
                               Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _downloadClientStatusService = downloadClientStatusService;
            _indexerFactory = indexerFactory;
            _indexerStatusService = indexerStatusService;
            _rateLimitService = rateLimitService;
            _eventAggregator = eventAggregator;
            _seedConfigProvider = seedConfigProvider;
            _logger = logger;
        }

        public void DownloadReport(RemoteMovie remoteMovie)
        {
            Ensure.That(remoteMovie.Movie, () => remoteMovie.Movie).IsNotNull();

            var downloadTitle = remoteMovie.Release.Title;
            var filterBlockedClients = remoteMovie.Release.PendingReleaseReason == PendingReleaseReason.DownloadClientUnavailable;
            var tags = remoteMovie.Movie?.Tags;
            var downloadClient = _downloadClientProvider.GetDownloadClient(remoteMovie.Release.DownloadProtocol, remoteMovie.Release.IndexerId, filterBlockedClients, tags);

            if (downloadClient == null)
            {
                throw new DownloadClientUnavailableException($"{remoteMovie.Release.DownloadProtocol} Download client isn't configured yet");
            }

            // Get the seed configuration for this release.
            remoteMovie.SeedConfiguration = _seedConfigProvider.GetSeedConfiguration(remoteMovie);

            // Limit grabs to 2 per second.
            if (remoteMovie.Release.DownloadUrl.IsNotNullOrWhiteSpace() && !remoteMovie.Release.DownloadUrl.StartsWith("magnet:"))
            {
                var url = new HttpUri(remoteMovie.Release.DownloadUrl);
                _rateLimitService.WaitAndPulse(url.Host, TimeSpan.FromSeconds(2));
            }

            IIndexer indexer = null;

            if (remoteMovie.Release.IndexerId > 0)
            {
                indexer = _indexerFactory.GetInstance(_indexerFactory.Get(remoteMovie.Release.IndexerId));
            }

            string downloadClientId;
            try
            {
                downloadClientId = downloadClient.Download(remoteMovie, indexer);
                _downloadClientStatusService.RecordSuccess(downloadClient.Definition.Id);
                _indexerStatusService.RecordSuccess(remoteMovie.Release.IndexerId);
            }
            catch (ReleaseUnavailableException)
            {
                _logger.Trace("Release {0} no longer available on indexer.", remoteMovie);
                throw;
            }
            catch (DownloadClientRejectedReleaseException)
            {
                _logger.Trace("Release {0} rejected by download client, possible duplicate.", remoteMovie);
                throw;
            }
            catch (ReleaseDownloadException ex)
            {
                var http429 = ex.InnerException as TooManyRequestsException;
                if (http429 != null)
                {
                    _indexerStatusService.RecordFailure(remoteMovie.Release.IndexerId, http429.RetryAfter);
                }
                else
                {
                    _indexerStatusService.RecordFailure(remoteMovie.Release.IndexerId);
                }

                throw;
            }

            var movieGrabbedEvent = new MovieGrabbedEvent(remoteMovie);
            movieGrabbedEvent.DownloadClient = downloadClient.Name;
            movieGrabbedEvent.DownloadClientId = downloadClient.Definition.Id;
            movieGrabbedEvent.DownloadClientName = downloadClient.Definition.Name;

            if (!string.IsNullOrWhiteSpace(downloadClientId))
            {
                movieGrabbedEvent.DownloadId = downloadClientId;
            }

            _logger.ProgressInfo("Report for {0} ({1}) sent to {2} from indexer {3}. {4}", remoteMovie.Movie.Title, remoteMovie.Movie.Year, downloadClient.Definition.Name, remoteMovie.Release.Indexer, downloadTitle);
            _eventAggregator.PublishEvent(movieGrabbedEvent);
        }
    }
}
