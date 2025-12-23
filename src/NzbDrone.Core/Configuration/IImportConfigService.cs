using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Configuration
{
    public interface IImportConfigService
    {
        bool AutoExtractArchives { get; set; }
        bool DeleteArchiveAfterExtraction { get; set; }
        bool SkipFreeSpaceCheckWhenImporting { get; set; }
        int MinimumFreeSpaceWhenImporting { get; set; }
        bool CopyUsingHardlinks { get; set; }
        bool EnableMediaInfo { get; set; }
        bool UseScriptImport { get; set; }
        string ScriptImportPath { get; set; }
        bool ImportExtraFiles { get; set; }
        string ExtraFileExtensions { get; set; }
        bool AutoRenameFolders { get; set; }
        RescanAfterRefreshType RescanAfterRefresh { get; set; }
        bool CreateEmptyMovieFolders { get; set; }
        bool DeleteEmptyFolders { get; set; }
        FileDateType FileDate { get; set; }
        bool AutoUnmonitorPreviouslyDownloadedMovies { get; set; }
        bool CleanupMetadataImages { get; set; }
    }
}
