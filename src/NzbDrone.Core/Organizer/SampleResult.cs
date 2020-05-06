using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Organizer
{
    public class SampleResult
    {
        public string FileName { get; set; }
        public Author Artist { get; set; }
        public Book Album { get; set; }
        public BookFile TrackFile { get; set; }
    }
}
