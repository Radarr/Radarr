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
        public ParsedMovieInfo FileMovieInfo { get; set; }
        public ParsedMovieInfo DownloadClientMovieInfo { get; set; }
        public ParsedMovieInfo FolderMovieInfo { get; set; }
        public Movie Movie { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public bool ExistingFile { get; set; }
        public bool SceneSource { get; set; }
        public string ReleaseGroup { get; set; }
        public string Edition { get; set; }


        public override string ToString()
        {
            return Path;
        }
    }
}
