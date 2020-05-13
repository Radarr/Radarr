using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Books;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class BookImportedEvent : IEvent
    {
        public Author Author { get; private set; }
        public Book Book { get; private set; }
        public List<BookFile> ImportedBooks { get; private set; }
        public List<BookFile> OldFiles { get; private set; }
        public bool NewDownload { get; private set; }
        public string DownloadClient { get; private set; }
        public string DownloadId { get; private set; }

        public BookImportedEvent(Author author, Book book, List<BookFile> importedBooks, List<BookFile> oldFiles, bool newDownload, DownloadClientItem downloadClientItem)
        {
            Author = author;
            Book = book;
            ImportedBooks = importedBooks;
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
