using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieImportedEvent : IEvent
    {
        public LocalMovie MovieInfo { get; private set; }
        public MovieFile ImportedMovie { get; private set; }
        public bool NewDownload { get; private set; }
        public string DownloadClient { get; private set; }
        public string DownloadId { get; private set; }
        public bool IsReadOnly { get; set; }

        public MovieImportedEvent(LocalMovie movieInfo, MovieFile importedMovie, bool newDownload)
        {
            MovieInfo = movieInfo;
            ImportedMovie = importedMovie;
            NewDownload = newDownload;
        }

        public MovieImportedEvent(LocalMovie movieInfo, MovieFile importedMovie, bool newDownload, string downloadClient, string downloadId, bool isReadOnly)
        {
            MovieInfo = movieInfo;
            ImportedMovie = importedMovie;
            NewDownload = newDownload;
            DownloadClient = downloadClient;
            DownloadId = downloadId;
            IsReadOnly = isReadOnly;
        }
    }
}
