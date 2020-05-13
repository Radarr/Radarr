using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookBookFile
    {
        public WebhookBookFile()
        {
        }

        public WebhookBookFile(BookFile bookFile)
        {
            Id = bookFile.Id;
            Path = bookFile.Path;
            Quality = bookFile.Quality.Quality.Name;
            QualityVersion = bookFile.Quality.Revision.Version;
            ReleaseGroup = bookFile.ReleaseGroup;
            SceneName = bookFile.SceneName;
        }

        public int Id { get; set; }
        public string Path { get; set; }
        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string SceneName { get; set; }
    }
}
