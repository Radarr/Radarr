using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieImportedEvent : IEvent
    {
        public LocalMovie MovieInfo { get; private set; }
        public MovieFile ImportedMovie { get; private set; }
        public bool NewDownload { get; private set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; private set; }

        public MovieImportedEvent(LocalMovie movieInfo, MovieFile importedMovie, bool newDownload)
        {
            MovieInfo = movieInfo;
            ImportedMovie = importedMovie;
            NewDownload = newDownload;
        }

        public MovieImportedEvent(LocalMovie movieInfo, MovieFile importedMovie, bool newDownload, DownloadClientItem downloadClientItem, string downloadId)
        {
            MovieInfo = movieInfo;
            ImportedMovie = importedMovie;
            NewDownload = newDownload;
            DownloadClientInfo = downloadClientItem.DownloadClientInfo;
            DownloadId = downloadId;
        }
    }
}
