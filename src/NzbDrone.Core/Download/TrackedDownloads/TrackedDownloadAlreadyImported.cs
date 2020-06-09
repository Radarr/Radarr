using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadAlreadyImported
    {
        bool IsImported(TrackedDownload trackedDownload, List<MovieHistory> historyItems);
    }

    public class TrackedDownloadAlreadyImported : ITrackedDownloadAlreadyImported
    {
        private readonly Logger _logger;

        public TrackedDownloadAlreadyImported(Logger logger)
        {
            _logger = logger;
        }

        public bool IsImported(TrackedDownload trackedDownload, List<MovieHistory> historyItems)
        {
            _logger.Trace("Checking if all movies for '{0}' have been imported", trackedDownload.DownloadItem.Title);

            if (historyItems.Empty())
            {
                _logger.Trace("No history for {0}", trackedDownload.DownloadItem.Title);
                return false;
            }

            var movie = trackedDownload.RemoteMovie.Movie;

            var lastHistoryItem = historyItems.FirstOrDefault(h => h.MovieId == movie.Id);

            if (lastHistoryItem == null)
            {
                _logger.Trace("No history for movie: {0}", movie.ToString());
                return false;
            }

            var allMoviesImportedInHistory = lastHistoryItem.EventType == MovieHistoryEventType.DownloadFolderImported;
            _logger.Trace("Last event for movie: {0} is: {1}", movie, lastHistoryItem.EventType);

            _logger.Trace("All movies for '{0}' have been imported: {1}", trackedDownload.DownloadItem.Title, allMoviesImportedInHistory);
            return allMoviesImportedInHistory;
        }
    }
}
