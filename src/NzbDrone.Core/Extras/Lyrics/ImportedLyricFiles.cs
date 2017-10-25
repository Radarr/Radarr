using System.Collections.Generic;
using NzbDrone.Core.Extras.Files;

namespace NzbDrone.Core.Extras.Lyrics
{
    public class ImportedLyricFiles
    {
        public List<string> SourceFiles { get; set; }
        public List<ExtraFile> LyricFiles { get; set; }

        public ImportedLyricFiles()
        {
            SourceFiles = new List<string>();
            LyricFiles = new List<ExtraFile>();
        }
    }
}
