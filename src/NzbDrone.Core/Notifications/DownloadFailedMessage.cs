using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Music;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Notifications
{
    public class DownloadFailedMessage
    {
        public string Message { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public Language Language { get; set; }
        public string DownloadClient { get; set; }
        public string DownloadId { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
