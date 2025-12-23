using NzbDrone.Core.MediaFiles;

namespace NzbDrone.Core.Configuration
{
    public class ImportConfigService : IImportConfigService
    {
        private readonly IConfigService _configService;

        public ImportConfigService(IConfigService configService)
        {
            _configService = configService;
        }

        public bool AutoExtractArchives
        {
            get => _configService.AutoExtractArchives;
            set => _configService.AutoExtractArchives = value;
        }

        public bool DeleteArchiveAfterExtraction
        {
            get => _configService.DeleteArchiveAfterExtraction;
            set => _configService.DeleteArchiveAfterExtraction = value;
        }

        public bool SkipFreeSpaceCheckWhenImporting
        {
            get => _configService.SkipFreeSpaceCheckWhenImporting;
            set => _configService.SkipFreeSpaceCheckWhenImporting = value;
        }

        public int MinimumFreeSpaceWhenImporting
        {
            get => _configService.MinimumFreeSpaceWhenImporting;
            set => _configService.MinimumFreeSpaceWhenImporting = value;
        }

        public bool CopyUsingHardlinks
        {
            get => _configService.CopyUsingHardlinks;
            set => _configService.CopyUsingHardlinks = value;
        }

        public bool EnableMediaInfo
        {
            get => _configService.EnableMediaInfo;
            set => _configService.EnableMediaInfo = value;
        }

        public bool UseScriptImport
        {
            get => _configService.UseScriptImport;
            set => _configService.UseScriptImport = value;
        }

        public string ScriptImportPath
        {
            get => _configService.ScriptImportPath;
            set => _configService.ScriptImportPath = value;
        }

        public bool ImportExtraFiles
        {
            get => _configService.ImportExtraFiles;
            set => _configService.ImportExtraFiles = value;
        }

        public string ExtraFileExtensions
        {
            get => _configService.ExtraFileExtensions;
            set => _configService.ExtraFileExtensions = value;
        }

        public bool AutoRenameFolders
        {
            get => _configService.AutoRenameFolders;
            set => _configService.AutoRenameFolders = value;
        }

        public RescanAfterRefreshType RescanAfterRefresh
        {
            get => _configService.RescanAfterRefresh;
            set => _configService.RescanAfterRefresh = value;
        }

        public bool CreateEmptyMovieFolders
        {
            get => _configService.CreateEmptyMovieFolders;
            set => _configService.CreateEmptyMovieFolders = value;
        }

        public bool DeleteEmptyFolders
        {
            get => _configService.DeleteEmptyFolders;
            set => _configService.DeleteEmptyFolders = value;
        }

        public FileDateType FileDate
        {
            get => _configService.FileDate;
            set => _configService.FileDate = value;
        }

        public bool AutoUnmonitorPreviouslyDownloadedMovies
        {
            get => _configService.AutoUnmonitorPreviouslyDownloadedMovies;
            set => _configService.AutoUnmonitorPreviouslyDownloadedMovies = value;
        }

        public bool CleanupMetadataImages
        {
            get => _configService.CleanupMetadataImages;
            set => _configService.CleanupMetadataImages = value;
        }
    }
}
