using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Download;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieDownloadedEvent : IEvent
    {
        public LocalMovie Movie { get; private set; }
        public MovieFile MovieFile { get; private set; }
        public List<MovieFile> OldFiles { get; private set; }
        public string DownloadId { get; private set; }

        public MovieDownloadedEvent(LocalMovie episode, MovieFile episodeFile, List<MovieFile> oldFiles, DownloadClientItem downloadClientItem)
        {
            Movie = episode;
            MovieFile = episodeFile;
            OldFiles = oldFiles;
            if (downloadClientItem != null)
            {
                DownloadId = downloadClientItem.DownloadId;
            }
        }
    }
}
