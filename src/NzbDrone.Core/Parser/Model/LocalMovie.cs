using System.Linq;
using System.Collections.Generic;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Movies;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Languages;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalMovie
    {
        public LocalMovie()
        {
        }

        public string Path { get; set; }
        public long Size { get; set; }
        public ParsedMovieInfo ParsedMovieInfo { get; set; }
        public Movie Movie { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public bool ExistingFile { get; set; }
        

        public override string ToString()
        {
            return Path;
        }
    }
}
