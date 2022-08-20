using NzbDrone.Core.Extras.Files;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Extras.Subtitles
{
    public class SubtitleFile : ExtraFile
    {
        public SubtitleFile()
        {
            LanguageTags = new List<string>();
        }

        public Language Language { get; set; }
    }
}
