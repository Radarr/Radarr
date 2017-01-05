using System;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Http;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Common.TPL;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IDownloadService
    {
        void DownloadReport(RemoteEpisode remoteEpisode);
        void DownloadReport(RemoteMovie remoteMovie);
    }


    public class DownloadService : IDownloadService
    {
        private readonly IProvideDownloadClient _downloadClientProvider;
        private readonly IIndexerStatusService _indexerStatusService;
        private readonly IRateLimitService _rateLimitService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DownloadService(IProvideDownloadClient downloadClientProvider,
            IIndexerStatusService indexerStatusService,
            IRateLimitService rateLimitService,
            IEventAggregator eventAggregator,
            Logger logger)
        {
            _downloadClientProvider = downloadClientProvider;
            _indexerStatusService = indexerStatusService;
            _rateLimitService = rateLimitService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void DownloadReport(RemoteEpisode remoteEpisode)
        {
            //Ensure.That(remoteEpisode.Series, () => remoteEpisode.Series).IsNotNull();
            //Ensure.That(remoteEpisode.Episodes, () => remoteEpisode.Episodes).HasItems(); TODO update this shit

            var downloadTitle = remoteEpisode.Release.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient(remoteEpisode.Release.DownloadProtocol);

            if (downloadClient == null)
            {
                _logger.Warn("{0} Download client isn't configured yet.", remoteEpisode.Release.DownloadProtocol);
                return;
            }

            // Limit grabs to 2 per second.
            if (remoteEpisode.Release.DownloadUrl.IsNotNullOrWhiteSpace() && !remoteEpisode.Release.DownloadUrl.StartsWith("magnet:"))
            {
                var url = new HttpUri(remoteEpisode.Release.DownloadUrl);
                _rateLimitService.WaitAndPulse(url.Host, TimeSpan.FromSeconds(2));
            }

            string downloadClientId;
            try
            {
                downloadClientId = downloadClient.Download(remoteEpisode);
                _indexerStatusService.RecordSuccess(remoteEpisode.Release.IndexerId);
            }
            catch (ReleaseDownloadException ex)
            {
                var http429 = ex.InnerException as TooManyRequestsException;
                if (http429 != null)
                {
                    _indexerStatusService.RecordFailure(remoteEpisode.Release.IndexerId, http429.RetryAfter);
                }
                else
                {
                    _indexerStatusService.RecordFailure(remoteEpisode.Release.IndexerId);
                }
                throw;
            }

            var episodeGrabbedEvent = new EpisodeGrabbedEvent(remoteEpisode);
            episodeGrabbedEvent.DownloadClient = downloadClient.GetType().Name;

            if (!string.IsNullOrWhiteSpace(downloadClientId))
            {
                episodeGrabbedEvent.DownloadId = downloadClientId;
            }

            _logger.ProgressInfo("Report sent to {0}. {1}", downloadClient.Definition.Name, downloadTitle);
            _eventAggregator.PublishEvent(episodeGrabbedEvent);
        }

        public void DownloadReport(RemoteMovie remoteMovie)
        {
            //Ensure.That(remoteEpisode.Series, () => remoteEpisode.Series).IsNotNull();
            //Ensure.That(remoteEpisode.Episodes, () => remoteEpisode.Episodes).HasItems(); TODO update this shit

            var downloadTitle = remoteMovie.Release.Title;
            var downloadClient = _downloadClientProvider.GetDownloadClient(remoteMovie.Release.DownloadProtocol);

            if (downloadClient == null)
            {
                _logger.Warn("{0} Download client isn't configured yet.", remoteMovie.Release.DownloadProtocol);
                return;
            }

            // Limit grabs to 2 per second.
            if (remoteMovie.Release.DownloadUrl.IsNotNullOrWhiteSpace() && !remoteMovie.Release.DownloadUrl.StartsWith("magnet:"))
            {
                var url = new HttpUri(remoteMovie.Release.DownloadUrl);
                _rateLimitService.WaitAndPulse(url.Host, TimeSpan.FromSeconds(2));
            }

            string downloadClientId = "";
            try
            {
                downloadClientId = downloadClient.Download(remoteMovie);
                _indexerStatusService.RecordSuccess(remoteMovie.Release.IndexerId);
            }
            catch (NotImplementedException ex)
            {
                _logger.Error(ex, "The download client you are using is currently not configured to download movies. Please choose another one.");
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

            var episodeGrabbedEvent = new MovieGrabbedEvent(remoteMovie);
            episodeGrabbedEvent.DownloadClient = downloadClient.GetType().Name;

            if (!string.IsNullOrWhiteSpace(downloadClientId))
            {
                episodeGrabbedEvent.DownloadId = downloadClientId;
            }

            _logger.ProgressInfo("Report sent to {0}. {1}", downloadClient.Definition.Name, downloadTitle);
            _eventAggregator.PublishEvent(episodeGrabbedEvent);
        }
    }
}