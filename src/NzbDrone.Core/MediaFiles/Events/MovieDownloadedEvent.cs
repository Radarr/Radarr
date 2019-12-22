using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieDownloadedEvent : IEvent
    {
        public LocalMovie Movie { get; private set; }
        public MovieFile MovieFile { get; private set; }
        public List<MovieFile> OldFiles { get; private set; }
        public string DownloadId { get; private set; }

        public MovieDownloadedEvent(LocalMovie movie, MovieFile movieFile, List<MovieFile> oldFiles, DownloadClientItem downloadClientItem)
        {
            Movie = movie;
            MovieFile = movieFile;
            OldFiles = oldFiles;
            if (downloadClientItem != null)
            {
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
