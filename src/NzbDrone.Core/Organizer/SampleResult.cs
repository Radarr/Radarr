using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Organizer
{
    public class SampleResult
    {
        public string FileName { get; set; }
        public Movie Movie { get; set; }
        public MovieFile MovieFile { get; set; }
    }
}
