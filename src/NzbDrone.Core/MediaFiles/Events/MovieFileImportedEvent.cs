using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieFileImportedEvent : IEvent
    {
        public LocalMovie MovieInfo { get; private set; }
        public MovieFile ImportedMovie { get; private set; }
        public List<DeletedMovieFile> OldFiles { get; private set; }
        public bool NewDownload { get; private set; }
        public DownloadClientItemClientInfo DownloadClientInfo { get; set; }
        public string DownloadId { get; private set; }

        public MovieFileImportedEvent(LocalMovie movieInfo, MovieFile importedMovie, List<DeletedMovieFile> oldFiles, bool newDownload, DownloadClientItem downloadClientItem)
        {
            MovieInfo = movieInfo;
            ImportedMovie = importedMovie;
            OldFiles = oldFiles;
            NewDownload = newDownload;
            if (downloadClientItem != null)
            {
                DownloadClientInfo = downloadClientItem.DownloadClientInfo;
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
