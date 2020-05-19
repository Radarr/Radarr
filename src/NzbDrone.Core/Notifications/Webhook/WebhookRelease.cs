using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRelease
    {
        public WebhookRelease()
        {
        }

        public WebhookRelease(QualityModel quality, RemoteBook remoteBook)
        {
            Quality = quality.Quality.Name;
            QualityVersion = quality.Revision.Version;
            ReleaseGroup = remoteBook.ParsedBookInfo.ReleaseGroup;
            ReleaseTitle = remoteBook.Release.Title;
            Indexer = remoteBook.Release.Indexer;
            Size = remoteBook.Release.Size;
        }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseTitle { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }
    }
}
