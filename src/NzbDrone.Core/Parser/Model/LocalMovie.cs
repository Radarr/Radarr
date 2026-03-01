using System;
using System.Collections.Generic;
using NzbDrone.Common.Extensions;
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
        public List<CustomFormat> CustomFormats { get; set; } = new();
        public int CustomFormatScore { get; set; }
        public List<CustomFormat> OriginalFileNameCustomFormats { get; set; } = new();
        public int OriginalFileNameCustomFormatScore { get; set; }
        public GrabbedReleaseInfo Release { get; set; }
        public bool ScriptImported { get; set; }
        public string FileNameBeforeRename { get; set; }
        public string FileNameUsedForCustomFormatCalculation { get; set; }
        public bool ShouldImportExtras { get; set; }
        public List<string> PossibleExtraFiles { get; set; }
        public SubtitleTitleInfo SubtitleInfo { get; set; }

        public override string ToString()
        {
            return Path;
        }

        public string GetSceneOrFileName()
        {
            if (SceneName.IsNotNullOrWhiteSpace())
            {
                return SceneName;
            }

            if (Path.IsNotNullOrWhiteSpace())
            {
                return System.IO.Path.GetFileNameWithoutExtension(Path);
            }

            return string.Empty;
        }

        public MovieFile ToMovieFile()
        {
            var movieFile = new MovieFile
            {
                DateAdded = DateTime.UtcNow,
                MovieId = Movie.Id,
                Path = Path.CleanFilePath(),
                Quality = Quality,
                MediaInfo = MediaInfo,
                Movie = Movie,
                ReleaseGroup = ReleaseGroup,
                Edition = Edition,
                Languages = Languages,
                IndexerFlags = IndexerFlags,
                SceneName = SceneName,
                OriginalFilePath = GetOriginalFilePath()
            };

            if (Movie.Path.IsParentPath(movieFile.Path))
            {
                movieFile.RelativePath = Movie.Path.GetRelativePath(Path.CleanFilePath());
            }

            return movieFile;
        }

        private string GetOriginalFilePath()
        {
            if (FolderMovieInfo != null)
            {
                var folderPath = Path.GetAncestorPath(FolderMovieInfo.OriginalTitle);

                if (folderPath != null)
                {
                    return folderPath.GetParentPath().GetRelativePath(Path);
                }
            }

            var parentPath = Path.GetParentPath();
            var grandparentPath = parentPath.GetParentPath();

            if (grandparentPath != null)
            {
                return grandparentPath.GetRelativePath(Path);
            }

            return System.IO.Path.GetFileName(Path);
        }
    }
}
