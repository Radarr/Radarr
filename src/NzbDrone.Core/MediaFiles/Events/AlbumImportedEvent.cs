using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class AlbumImportedEvent : IEvent
    {
        public Artist Artist { get; private set; }
        public Album Album { get; private set; }
        public List<LocalTrack> ImportedTracks { get; private set; }
        public bool NewDownload { get; private set; }
        public string DownloadClient { get; private set; }
        public string DownloadId { get; private set; }

        public AlbumImportedEvent(Artist artist, Album album, List<LocalTrack> importedTracks, bool newDownload, DownloadClientItem downloadClientItem)
        {
            Artist = artist;
            Album = album;
            ImportedTracks = importedTracks;
            NewDownload = newDownload;

            if (downloadClientItem != null)
            {
                DownloadClient = downloadClientItem.DownloadClient;
                DownloadId = downloadClientItem.DownloadId;
            }

        }
    }
}
