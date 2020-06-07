using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles;
using Radarr.Http.REST;

namespace NzbDrone.Api.Config
{
    public class MediaManagementConfigResource : RestResource
    {
        public bool AutoUnmonitorPreviouslyDownloadedEpisodes { get; set; }
        public string RecycleBin { get; set; }
        public bool AutoDownloadPropers { get; set; }
        public bool CreateEmptySeriesFolders { get; set; }
        public FileDateType FileDate { get; set; }
        public bool AutoRenameFolders { get; set; }
        public bool PathsDefaultStatic { get; set; }

        public bool SetPermissionsLinux { get; set; }
        public string FileChmod { get; set; }

        public bool SkipFreeSpaceCheckWhenImporting { get; set; }
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
                AutoUnmonitorPreviouslyDownloadedEpisodes = model.AutoUnmonitorPreviouslyDownloadedMovies,
                RecycleBin = model.RecycleBin,
                AutoDownloadPropers = model.AutoDownloadPropers,
                CreateEmptySeriesFolders = model.CreateEmptyMovieFolders,
                FileDate = model.FileDate,
                AutoRenameFolders = model.AutoRenameFolders,

                SetPermissionsLinux = model.SetPermissionsLinux,
                FileChmod = model.FileChmod,

                SkipFreeSpaceCheckWhenImporting = model.SkipFreeSpaceCheckWhenImporting,
                CopyUsingHardlinks = model.CopyUsingHardlinks,
                ImportExtraFiles = model.ImportExtraFiles,
                ExtraFileExtensions = model.ExtraFileExtensions,
                EnableMediaInfo = model.EnableMediaInfo
            };
        }
    }
}
