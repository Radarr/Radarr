using System;
using NLog;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.Clients;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        void DownloadReport(RemoteAlbum remoteAlbum);
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

        public void DownloadReport(RemoteAlbum remoteAlbum)
        {
            Ensure.That(remoteAlbum.Artist, () => remoteAlbum.Artist).IsNotNull();
            Ensure.That(remoteAlbum.Albums, () => remoteAlbum.Albums).HasItems();

            var downloadTitle = remoteAlbum.Release.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient(remoteAlbum.Release.DownloadProtocol);

            if (downloadClient == null)
            {
                throw new DownloadClientUnavailableException($"{remoteAlbum.Release.DownloadProtocol} Download client isn't configured yet");
            }

            // Get the seed configuration for this release.
            remoteAlbum.SeedConfiguration = _seedConfigProvider.GetSeedConfiguration(remoteAlbum);

            // Limit grabs to 2 per second.
            if (remoteAlbum.Release.DownloadUrl.IsNotNullOrWhiteSpace() && !remoteAlbum.Release.DownloadUrl.StartsWith("magnet:"))
            {
                var url = new HttpUri(remoteAlbum.Release.DownloadUrl);
                _rateLimitService.WaitAndPulse(url.Host, TimeSpan.FromSeconds(2));
            }

            string downloadClientId;
            try
            {
                downloadClientId = downloadClient.Download(remoteAlbum);
                _downloadClientStatusService.RecordSuccess(downloadClient.Definition.Id);
                _indexerStatusService.RecordSuccess(remoteAlbum.Release.IndexerId);
            }
            catch (ReleaseUnavailableException)
            {
                _logger.Trace("Release {0} no longer available on indexer.", remoteAlbum);
                throw;
            }
            catch (ReleaseDownloadException ex)
            {
                var http429 = ex.InnerException as TooManyRequestsException;
                if (http429 != null)
                {
                    _indexerStatusService.RecordFailure(remoteAlbum.Release.IndexerId, http429.RetryAfter);
                }
                else
                {
                    _indexerStatusService.RecordFailure(remoteAlbum.Release.IndexerId);
                }
                throw;
            }

            var albumGrabbedEvent = new AlbumGrabbedEvent(remoteAlbum);
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
