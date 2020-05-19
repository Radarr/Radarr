using System;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        void DownloadReport(RemoteBook remoteBook);
    }

    public class DownloadService : IDownloadService
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IDownloadClientStatusService _downloadClientStatusService;
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ISeedConfigProvider _seedConfigProvider;
        private readonly Logger _logger;

        public DownloadService(IProvideDownloadClient downloadClientProvider,
                               IDownloadClientStatusService downloadClientStatusService,
                               IIndexerStatusService indexerStatusService,
                               IRateLimitService rateLimitService,
                               IEventAggregator eventAggregator,
                               ISeedConfigProvider seedConfigProvider,
                               Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _downloadClientStatusService = downloadClientStatusService;
            _indexerStatusService = indexerStatusService;
            _rateLimitService = rateLimitService;
            _eventAggregator = eventAggregator;
            _seedConfigProvider = seedConfigProvider;
            _logger = logger;
        }

        public void DownloadReport(RemoteBook remoteBook)
        {
            Ensure.That(remoteBook.Author, () => remoteBook.Author).IsNotNull();
            Ensure.That(remoteBook.Books, () => remoteBook.Books).HasItems();

            var downloadTitle = remoteBook.Release.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient(remoteBook.Release.DownloadProtocol);

            if (downloadClient == null)
            {
                throw new DownloadClientUnavailableException($"{remoteBook.Release.DownloadProtocol} Download client isn't configured yet");
            }

            // Get the seed configuration for this release.
            remoteBook.SeedConfiguration = _seedConfigProvider.GetSeedConfiguration(remoteBook);

            // Limit grabs to 2 per second.
            if (remoteBook.Release.DownloadUrl.IsNotNullOrWhiteSpace() && !remoteBook.Release.DownloadUrl.StartsWith("magnet:"))
            {
                var url = new HttpUri(remoteBook.Release.DownloadUrl);
                _rateLimitService.WaitAndPulse(url.Host, TimeSpan.FromSeconds(2));
            }

            string downloadClientId;
            try
            {
                downloadClientId = downloadClient.Download(remoteBook);
                _downloadClientStatusService.RecordSuccess(downloadClient.Definition.Id);
                _indexerStatusService.RecordSuccess(remoteBook.Release.IndexerId);
            }
            catch (ReleaseUnavailableException)
            {
                _logger.Trace("Release {0} no longer available on indexer.", remoteBook);
                throw;
            }
            catch (ReleaseDownloadException ex)
            {
                var http429 = ex.InnerException as TooManyRequestsException;
                if (http429 != null)
                {
                    _indexerStatusService.RecordFailure(remoteBook.Release.IndexerId, http429.RetryAfter);
                }
                else
                {
                    _indexerStatusService.RecordFailure(remoteBook.Release.IndexerId);
                }

                throw;
            }

            var albumGrabbedEvent = new BookGrabbedEvent(remoteBook);
            albumGrabbedEvent.DownloadClient = downloadClient.Name;

            if (!string.IsNullOrWhiteSpace(downloadClientId))
            {
                albumGrabbedEvent.DownloadId = downloadClientId;
            }

            _logger.ProgressInfo("Report sent to {0}. {1}", downloadClient.Definition.Name, downloadTitle);
            _eventAggregator.PublishEvent(albumGrabbedEvent);
        }
    }
}
