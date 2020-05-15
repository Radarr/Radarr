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

            if (trackedDownload.RemoteBook == null || trackedDownload.RemoteBook.Books == null)
            {
                return true;
            }

            var allAlbumsImportedInHistory = trackedDownload.RemoteBook.Books.All(book =>
            {
                var lastHistoryItem = historyItems.FirstOrDefault(h => h.BookId == book.Id);

                if (lastHistoryItem == null)
                {
                    return false;
                }

                return new[] { HistoryEventType.DownloadImported, HistoryEventType.BookFileImported }.Contains(lastHistoryItem.EventType);
            });

            return allAlbumsImportedInHistory;
        }
    }
}
