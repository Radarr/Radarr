using NzbDrone.Core.Books;
using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Organizer
{
    public class SampleResult
    {
        public string FileName { get; set; }
        public Author Author { get; set; }
        public Book Book { get; set; }
        public BookFile BookFile { get; set; }
    }
}
