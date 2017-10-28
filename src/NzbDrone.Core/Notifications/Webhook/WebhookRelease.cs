using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRelease
    {
        public WebhookRelease() { }

        public WebhookRelease(QualityModel quality, RemoteMovie remoteMovie)
        {
            Quality = quality.Quality.Name;
            QualityVersion = quality.Revision.Version;
            ReleaseGroup = remoteMovie.ParsedMovieInfo.ReleaseGroup;
            ReleaseTitle = remoteMovie.Release.Title;
            Indexer = remoteMovie.Release.Indexer;
            Size = remoteMovie.Release.Size;
        }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseTitle { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }
    }
}
