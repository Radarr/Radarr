using System;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications
{
    public class TrackRetagMessage
    {
        public string Message { get; set; }
        public Artist Artist { get; set; }
        public Album Album { get; set; }
        public AlbumRelease Release { get; set; }
        public TrackFile TrackFile { get; set; }
        public Dictionary<string, Tuple<string, string>> Diff { get; set; }
        public bool Scrubbed { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
