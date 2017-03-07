﻿using NzbDrone.Api.REST;
using NzbDrone.Core.Configuration;

namespace NzbDrone.Api.Config
{
    public class DownloadClientConfigResource : RestResource
    {
        public string DownloadedMoviesFolder { get; set; }
        public string DownloadClientWorkingFolders { get; set; }
        public int DownloadedMoviesScanInterval { get; set; }

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
                DownloadedMoviesFolder = model.DownloadedMoviesFolder,
                DownloadClientWorkingFolders = model.DownloadClientWorkingFolders,
                DownloadedMoviesScanInterval = model.DownloadedMoviesScanInterval,

                EnableCompletedDownloadHandling = model.EnableCompletedDownloadHandling,
                RemoveCompletedDownloads = model.RemoveCompletedDownloads,

                AutoRedownloadFailed = model.AutoRedownloadFailed,
                RemoveFailedDownloads = model.RemoveFailedDownloads
            };
        }
    }
}
