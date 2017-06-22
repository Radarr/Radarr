using NzbDrone.Common.Messaging;
using NzbDrone.Core.Parser.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles.Events
{
    public class TrackDownloadedEvent : IEvent
    {
        public LocalTrack Track { get; private set; }
        public TrackFile TrackFile { get; private set; }
        public List<TrackFile> OldFiles { get; private set; }

        public TrackDownloadedEvent(LocalTrack track, TrackFile trackFile, List<TrackFile> oldFiles)
        {
            Track = track;
            TrackFile = trackFile;
            OldFiles = oldFiles;
        }
    }
}
