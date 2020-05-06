using System;
using System.Collections.Generic;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Notifications
{
    public class TrackRetagMessage
    {
        public string Message { get; set; }
        public Author Artist { get; set; }
        public Book Album { get; set; }
        public BookFile TrackFile { get; set; }
        public Dictionary<string, Tuple<string, string>> Diff { get; set; }
        public bool Scrubbed { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
