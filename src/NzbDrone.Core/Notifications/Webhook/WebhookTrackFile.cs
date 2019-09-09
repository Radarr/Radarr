using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookTrackFile
    {
        public WebhookTrackFile() { }

        public WebhookTrackFile(TrackFile trackFile)
        {
            Id = trackFile.Id;
            Path = trackFile.Path;
            Quality = trackFile.Quality.Quality.Name;
            QualityVersion = trackFile.Quality.Revision.Version;
            ReleaseGroup = trackFile.ReleaseGroup;
            SceneName = trackFile.SceneName;
        }

        public int Id { get; set; }
        public string Path { get; set; }
        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string SceneName { get; set; }
    }
}
