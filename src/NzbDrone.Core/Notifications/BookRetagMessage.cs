using System;
using System.Collections.Generic;
using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Notifications
{
    public class BookRetagMessage
    {
        public string Message { get; set; }
        public Author Author { get; set; }
        public Book Book { get; set; }
        public BookFile BookFile { get; set; }
        public Dictionary<string, Tuple<string, string>> Diff { get; set; }
        public bool Scrubbed { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
