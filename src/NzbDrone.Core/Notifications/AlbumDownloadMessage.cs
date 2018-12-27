using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications
{
    public class AlbumDownloadMessage
    {
        public string Message { get; set; }
        public Artist Artist { get; set; }
        public Album Album { get; set; }
        public AlbumRelease Release { get; set; }
        public List<TrackFile> TrackFiles { get; set; }
        public List<TrackFile> OldFiles { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
