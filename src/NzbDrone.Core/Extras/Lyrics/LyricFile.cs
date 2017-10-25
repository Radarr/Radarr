using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Lyrics
{
    public class LyricFile : ExtraFile
    {
        public Language Language { get; set; }
    }
}
