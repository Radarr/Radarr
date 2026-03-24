using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Notifications.Webhook
{
    public class WebhookRelease
    {
        public WebhookRelease()
        {
        }

        public WebhookRelease(QualityModel quality, RemoteMovie remoteMovie)
        {
            Quality = quality.Quality.Name;
            QualityVersion = quality.Revision.Version;
            ReleaseGroup = remoteMovie.ParsedMovieInfo.ReleaseGroup;
            ReleaseTitle = remoteMovie.Release.Title;
            Indexer = remoteMovie.Release.Indexer;
            Size = remoteMovie.Release.Size;
            CustomFormats = remoteMovie.CustomFormats?.Select(x => x.Name).ToList();
            CustomFormatScore = remoteMovie.CustomFormatScore;
            Languages = remoteMovie.Languages;
            IndexerFlags = Enum.GetValues(typeof(IndexerFlags)).Cast<IndexerFlags>().Where(f => (remoteMovie.Release.IndexerFlags & f) == f).Select(f => f.ToString()).ToList();
        }

        public string Quality { get; set; }
        public int QualityVersion { get; set; }
        public string ReleaseGroup { get; set; }
        public string ReleaseTitle { get; set; }
        public string Indexer { get; set; }
        public long Size { get; set; }
        public int CustomFormatScore { get; set; }
        public List<string> CustomFormats { get; set; }
        public List<Language> Languages { get; set; }
        public List<string> IndexerFlags { get; set; }
    }
}
