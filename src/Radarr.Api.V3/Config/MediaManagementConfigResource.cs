using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Config
{
    public class MediaManagementConfigResource : RestResource
    {
        public bool AutoUnmonitorPreviouslyDownloadedMovies { get; set; }
        public string RecycleBin { get; set; }
        public int RecycleBinCleanupDays { get; set; }
        public bool AutoDownloadPropers { get; set; }
        public bool CreateEmptyMovieFolders { get; set; }
        public bool DeleteEmptyFolders { get; set; }
        public FileDateType FileDate { get; set; }
        public RescanAfterRefreshType RescanAfterRefresh { get; set; }
        public bool AutoRenameFolders { get; set; }
        public bool PathsDefaultStatic { get; set; }

        public bool SetPermissionsLinux { get; set; }
        public string FileChmod { get; set; }
        public string FolderChmod { get; set; }
        public string ChownUser { get; set; }
        public string ChownGroup { get; set; }

        public bool SkipFreeSpaceCheckWhenImporting { get; set; }
        public int MinimumFreeSpaceWhenImporting { get; set; }
        public bool CopyUsingHardlinks { get; set; }
        public bool ImportExtraFiles { get; set; }
        public string ExtraFileExtensions { get; set; }
        public bool EnableMediaInfo { get; set; }
    }

    public static class MediaManagementConfigResourceMapper
    {
        public static MediaManagementConfigResource ToResource(IConfigService model)
        {
            return new MediaManagementConfigResource
            {
                AutoUnmonitorPreviouslyDownloadedMovies = model.AutoUnmonitorPreviouslyDownloadedMovies,
                RecycleBin = model.RecycleBin,
                RecycleBinCleanupDays = model.RecycleBinCleanupDays,
                AutoDownloadPropers = model.AutoDownloadPropers,
                CreateEmptyMovieFolders = model.CreateEmptyMovieFolders,
                DeleteEmptyFolders = model.DeleteEmptyFolders,
                FileDate = model.FileDate,
                RescanAfterRefresh = model.RescanAfterRefresh,
                AutoRenameFolders = model.AutoRenameFolders,

                SetPermissionsLinux = model.SetPermissionsLinux,
                FileChmod = model.FileChmod,
                FolderChmod = model.FolderChmod,
                ChownUser = model.ChownUser,
                ChownGroup = model.ChownGroup,

                SkipFreeSpaceCheckWhenImporting = model.SkipFreeSpaceCheckWhenImporting,
                MinimumFreeSpaceWhenImporting = model.MinimumFreeSpaceWhenImporting,
                CopyUsingHardlinks = model.CopyUsingHardlinks,
                ImportExtraFiles = model.ImportExtraFiles,
                ExtraFileExtensions = model.ExtraFileExtensions,
                EnableMediaInfo = model.EnableMediaInfo
            };
        }
    }
}
