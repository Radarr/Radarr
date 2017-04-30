using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackImportedEvent : IEvent
    {
        public LocalTrack TrackInfo { get; private set; }
        public TrackFile ImportedTrack { get; private set; }
        public bool NewDownload { get; private set; }
        public string DownloadClient { get; private set; }
        public string DownloadId { get; private set; }
        public bool IsReadOnly { get; set; }

        public TrackImportedEvent(LocalTrack trackInfo, TrackFile importedTrack, bool newDownload)
        {
            TrackInfo = trackInfo;
            ImportedTrack = importedTrack;
            NewDownload = newDownload;
        }

        public TrackImportedEvent(LocalTrack trackInfo, TrackFile importedTrack, bool newDownload, string downloadClient, string downloadId, bool isReadOnly)
        {
            TrackInfo = trackInfo;
            ImportedTrack = importedTrack;
            NewDownload = newDownload;
            DownloadClient = downloadClient;
            DownloadId = downloadId;
            IsReadOnly = isReadOnly;
        }
    }
}
