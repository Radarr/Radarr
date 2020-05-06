using NzbDrone.Core.Music;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Notifications
{
    public class GrabMessage
    {
        public string Message { get; set; }
        public Author Artist { get; set; }
        public RemoteAlbum Album { get; set; }
        public QualityModel Quality { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
