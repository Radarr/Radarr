using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRelease
    {
        public WebhookRelease() { }

        public WebhookRelease(QualityModel quality, RemoteAlbum remoteAlbum)
        {
            Quality = quality.Quality.Name;
            QualityVersion = quality.Revision.Version;
            ReleaseGroup = remoteAlbum.ParsedAlbumInfo.ReleaseGroup;
            ReleaseTitle = remoteAlbum.Release.Title;
            Indexer = remoteAlbum.Release.Indexer;
            Size = remoteAlbum.Release.Size;
        }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseTitle { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }
    }
}
