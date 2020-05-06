using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class AlbumImportedEvent : IEvent
    {
        public Author Artist { get; private set; }
        public Book Album { get; private set; }
        public List<BookFile> ImportedTracks { get; private set; }
        public List<BookFile> OldFiles { get; private set; }
        public bool NewDownload { get; private set; }
        public string DownloadClient { get; private set; }
        public string DownloadId { get; private set; }

        public AlbumImportedEvent(Author artist, Book album, List<BookFile> importedTracks, List<BookFile> oldFiles, bool newDownload, DownloadClientItem downloadClientItem)
        {
            Artist = artist;
            Album = album;
            ImportedTracks = importedTracks;
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
