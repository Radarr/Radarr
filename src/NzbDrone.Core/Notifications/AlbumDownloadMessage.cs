using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications
{
    public class AlbumDownloadMessage
    {
        public string Message { get; set; }
        public Author Artist { get; set; }
        public Book Album { get; set; }
        public List<BookFile> TrackFiles { get; set; }
        public List<BookFile> OldFiles { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
