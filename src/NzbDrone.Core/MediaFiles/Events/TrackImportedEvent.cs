using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackImportedEvent : IEvent
    {
        public LocalBook BookInfo { get; private set; }
        public BookFile ImportedBook { get; private set; }
        public List<BookFile> OldFiles { get; private set; }
        public bool NewDownload { get; private set; }
        public string DownloadClient { get; private set; }
        public string DownloadId { get; private set; }

        public TrackImportedEvent(LocalBook bookInfo, BookFile importedBook, List<BookFile> oldFiles, bool newDownload, DownloadClientItem downloadClientItem)
        {
            BookInfo = bookInfo;
            ImportedBook = importedBook;
            OldFiles = oldFiles;
            NewDownload = newDownload;

            if (downloadClientItem != null)
            {
                DownloadClient = downloadClientItem.DownloadClient;
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
