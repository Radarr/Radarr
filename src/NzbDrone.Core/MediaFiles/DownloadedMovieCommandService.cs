using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Messaging.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace NzbDrone.Core.MediaFiles
{
    public class DownloadedMovieCommandService : IExecute<DownloadedMoviesScanCommand>
    {
        private readonly IDownloadedMovieImportService _downloadedMovieImportService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DownloadedMovieCommandService(IDownloadedMovieImportService downloadedMovieImportService,
                                                ITrackedDownloadService trackedDownloadService,
                                                IDiskProvider diskProvider,
                                                IConfigService configService,
                                                Logger logger)
        {
            _downloadedMovieImportService = downloadedMovieImportService;
            _trackedDownloadService = trackedDownloadService;
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        private List<ImportResult> ProcessDroneFactoryFolder()
        {
            var downloadedMoviesFolder = _configService.DownloadedMoviesFolder;

            if (string.IsNullOrEmpty(downloadedMoviesFolder))
            {
                _logger.Trace("Drone Factory folder is not configured");
                return new List<ImportResult>();
            }

            if (!_diskProvider.FolderExists(downloadedMoviesFolder))
            {
                _logger.Warn("Drone Factory folder [{0}] doesn't exist.", downloadedMoviesFolder);
                return new List<ImportResult>();
            }

            return _downloadedMovieImportService.ProcessRootFolder(new DirectoryInfo(downloadedMoviesFolder));
        }

        private List<ImportResult> ProcessPath(DownloadedMoviesScanCommand message)
        {
            if (!_diskProvider.FolderExists(message.Path) && !_diskProvider.FileExists(message.Path))
            {
                _logger.Warn("Folder/File specified for import scan [{0}] doesn't exist.", message.Path);
                return new List<ImportResult>();
            }

            if (message.DownloadClientId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(message.DownloadClientId);

                if (trackedDownload != null)
                {
                    _logger.Debug("External directory scan request for known download {0}. [{1}]", message.DownloadClientId, message.Path);

                    return _downloadedMovieImportService.ProcessPath(message.Path, message.ImportMode, trackedDownload.RemoteMovie.Movie, trackedDownload.DownloadItem);
                }
                else
                {
                    _logger.Warn("External directory scan request for unknown download {0}, attempting normal import. [{1}]", message.DownloadClientId, message.Path);

                    return _downloadedMovieImportService.ProcessPath(message.Path, message.ImportMode);
                }
            }

            return _downloadedMovieImportService.ProcessPath(message.Path, message.ImportMode);
        }

        public void Execute(DownloadedMoviesScanCommand message)
        {
            List<ImportResult> importResults;

            if (message.Path.IsNotNullOrWhiteSpace())
            {
                importResults = ProcessPath(message);
            }
            else
            {
                importResults = ProcessDroneFactoryFolder();
            }

            if (importResults == null || importResults.All(v => v.Result != ImportResultType.Imported))
            {
                // Atm we don't report it as a command failure, coz that would cause the download to be failed.
                // Changing the message won't do a thing either, coz it will get set to 'Completed' a msec later.
                //message.SetMessage("Failed to import");
            }
        }
    }
}
