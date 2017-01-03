using System.Collections.Generic;
using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class MovieDownloadedEvent : IEvent
    {
        public LocalMovie Movie { get; private set; }
        public MovieFile MovieFile { get; private set; }
        public List<MovieFile> OldFiles { get; private set; }

        public MovieDownloadedEvent(LocalMovie episode, MovieFile episodeFile, List<MovieFile> oldFiles)
        {
            Movie = episode;
            MovieFile = episodeFile;
            OldFiles = oldFiles;
        }
    }
}