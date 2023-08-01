using System.Collections.Generic;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Parser.Model
{
    public class LocalMovie
    {
        public LocalMovie()
        {
            CustomFormats = new List<CustomFormat>();
        }

        public string Path { get; set; }
        public long Size { get; set; }
        public ParsedMovieInfo FileMovieInfo { get; set; }
        public ParsedMovieInfo DownloadClientMovieInfo { get; set; }
        public DownloadClientItem DownloadItem { get; set; }
        public ParsedMovieInfo FolderMovieInfo { get; set; }
        public Movie Movie { get; set; }
        public List<DeletedMovieFile> OldFiles { get; set; }
        public QualityModel Quality { get; set; }
        public List<Language> Languages { get; set; }
        public IndexerFlags IndexerFlags { get; set; }
        public MediaInfoModel MediaInfo { get; set; }
        public bool ExistingFile { get; set; }
        public bool SceneSource { get; set; }
        public string ReleaseGroup { get; set; }
        public string Edition { get; set; }
        public string SceneName { get; set; }
        public bool OtherVideoFiles { get; set; }
        public List<CustomFormat> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public GrabbedReleaseInfo Release { get; set; }
        public bool ScriptImported { get; set; }
        public bool FileRenamedAfterScriptImport { get; set; }
        public bool ShouldImportExtras { get; set; }
        public List<string> PossibleExtraFiles { get; set; }

        public override string ToString()
        {
            return Path;
        }
    }
}
