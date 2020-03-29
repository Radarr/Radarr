using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadAlreadyImported
    {
        bool IsImported(TrackedDownload trackedDownload, List<History.History> historyItems);
    }

    public class TrackedDownloadAlreadyImported : ITrackedDownloadAlreadyImported
    {
        public bool IsImported(TrackedDownload trackedDownload, List<History.History> historyItems)
        {
            if (historyItems.Empty())
            {
                return false;
            }

            var movie = trackedDownload.RemoteMovie.Movie;

            var lastHistoryItem = historyItems.FirstOrDefault(h => h.MovieId == movie.Id);

            if (lastHistoryItem == null)
            {
                return false;
            }

            var allEpisodesImportedInHistory = lastHistoryItem.EventType == HistoryEventType.DownloadFolderImported;

            return allEpisodesImportedInHistory;
        }
    }
}
