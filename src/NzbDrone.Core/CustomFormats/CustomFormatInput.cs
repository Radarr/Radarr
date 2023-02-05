using System.Collections.Generic;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.CustomFormats
{
    public class CustomFormatInput
    {
        public ParsedMovieInfo MovieInfo { get; set; }
        public Movie Movie { get; set; }
        public long Size { get; set; }
        public IndexerFlags IndexerFlags { get; set; }
        public List<Language> Languages { get; set; }
        public string Filename { get; set; }

        public CustomFormatInput()
        {
            Languages = new List<Language>();
        }
    }
}
