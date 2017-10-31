using NzbDrone.Core.Configuration;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.Config
{
    public class DownloadClientConfigResource : RestResource
    {
        public string DownloadClientWorkingFolders { get; set; }

        public bool EnableCompletedDownloadHandling { get; set; }
        public bool RemoveCompletedDownloads { get; set; }

        public bool AutoRedownloadFailed { get; set; }
        public bool RemoveFailedDownloads { get; set; }
    }

    public static class DownloadClientConfigResourceMapper
    {
        public static DownloadClientConfigResource ToResource(IConfigService model)
        {
            return new DownloadClientConfigResource
            {
                DownloadClientWorkingFolders = model.DownloadClientWorkingFolders,

                EnableCompletedDownloadHandling = model.EnableCompletedDownloadHandling,
                RemoveCompletedDownloads = model.RemoveCompletedDownloads,

                AutoRedownloadFailed = model.AutoRedownloadFailed,
                RemoveFailedDownloads = model.RemoveFailedDownloads
            };
        }
    }
}
