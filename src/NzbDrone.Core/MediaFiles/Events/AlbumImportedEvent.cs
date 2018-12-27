using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class AlbumImportedEvent : IEvent
    {
        public Artist Artist { get; private set; }
        public Album Album { get; private set; }
        public AlbumRelease AlbumRelease { get; private set; }
        public List<TrackFile> ImportedTracks { get; private set; }
        public List<TrackFile> OldFiles { get; private set; }
        public bool NewDownload { get; private set; }
        public string DownloadClient { get; private set; }
        public string DownloadId { get; private set; }

        public AlbumImportedEvent(Artist artist, Album album, AlbumRelease release, List<TrackFile> importedTracks, List<TrackFile> oldFiles, bool newDownload, DownloadClientItem downloadClientItem)
        {
            Artist = artist;
            Album = album;
            AlbumRelease = release;
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
