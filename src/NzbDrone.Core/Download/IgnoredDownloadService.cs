using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
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
            var artist = trackedDownload.RemoteAlbum.Artist;
            var albums = trackedDownload.RemoteAlbum.Albums;

            if (artist == null || albums.Empty())
            {
                _logger.Warn("Unable to ignore download for unknown artist/album");
                return false;
            }

            var downloadIgnoredEvent = new DownloadIgnoredEvent
                                      {
                                          AuthorId = artist.Id,
                                          BookIds = albums.Select(e => e.Id).ToList(),
                                          Quality = trackedDownload.RemoteAlbum.ParsedAlbumInfo.Quality,
                                          SourceTitle = trackedDownload.DownloadItem.Title,
                                          DownloadClient = trackedDownload.DownloadItem.DownloadClient,
                                          DownloadId = trackedDownload.DownloadItem.DownloadId,
                                          Message = "Manually ignored"
                                      };

            _eventAggregator.PublishEvent(downloadIgnoredEvent);
            return true;
        }
    }
}
