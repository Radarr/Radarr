using NLog;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public interface IIgnoredDownloadService
    {
        bool IgnoreDownload(TrackedDownload trackedDownload);
    }

    public class IgnoredDownloadService : IIgnoredDownloadService
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public IgnoredDownloadService(IEventAggregator eventAggregator,
                                      Logger logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public bool IgnoreDownload(TrackedDownload trackedDownload)
        {
            var movie = trackedDownload.RemoteMovie.Movie;

            if (movie == null)
            {
                _logger.Warn("Unable to ignore download for unknown movie");
                return false;
            }

            var downloadIgnoredEvent = new DownloadIgnoredEvent
            {
                MovieId = movie.Id,
                Languages = trackedDownload.RemoteMovie.ParsedMovieInfo.Languages,
                Quality = trackedDownload.RemoteMovie.ParsedMovieInfo.Quality,
                SourceTitle = trackedDownload.DownloadItem.Title,
                DownloadClientInfo = trackedDownload.DownloadItem.DownloadClientInfo,
                DownloadId = trackedDownload.DownloadItem.DownloadId,
                TrackedDownload = trackedDownload,
                Message = "Manually ignored"
            };

            _eventAggregator.PublishEvent(downloadIgnoredEvent);
            return true;
        }
    }
}
