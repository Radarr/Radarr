using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MediaFiles.TrackImport;

namespace NzbDrone.Core.MediaFiles
{
    public class DownloadedAlbumsCommandService : IExecute<DownloadedAlbumsScanCommand>
    {
        private readonly IDownloadedEpisodesImportService _downloadedEpisodesImportService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDiskProvider _diskProvider;
        private readonly IConfigService _configService;
        private readonly Logger _logger;

        public DownloadedAlbumsCommandService(IDownloadedEpisodesImportService downloadedEpisodesImportService,
                                                ITrackedDownloadService trackedDownloadService,
                                                IDiskProvider diskProvider,
                                                IConfigService configService,
                                                Logger logger)
        {
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _trackedDownloadService = trackedDownloadService;
            _diskProvider = diskProvider;
            _configService = configService;
            _logger = logger;
        }

        private List<ImportResult> ProcessDroneFactoryFolder()
        {
            var downloadedAlbumsFolder = _configService.DownloadedAlbumsFolder;

            if (string.IsNullOrEmpty(downloadedAlbumsFolder))
            {
                _logger.Trace("Drone Factory folder is not configured");
                return new List<ImportResult>();
            }

            if (!_diskProvider.FolderExists(downloadedAlbumsFolder))
            {
                _logger.Warn("Drone Factory folder [{0}] doesn't exist.", downloadedAlbumsFolder);
                return new List<ImportResult>();
            }

            return _downloadedEpisodesImportService.ProcessRootFolder(new DirectoryInfo(downloadedAlbumsFolder));
        }

        private List<ImportResult> ProcessPath(DownloadedAlbumsScanCommand message)
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

                    return _downloadedEpisodesImportService.ProcessPath(message.Path, message.ImportMode, trackedDownload.RemoteEpisode.Series, trackedDownload.DownloadItem);
                }
                else
                {
                    _logger.Warn("External directory scan request for unknown download {0}, attempting normal import. [{1}]", message.DownloadClientId, message.Path);

                    return _downloadedEpisodesImportService.ProcessPath(message.Path, message.ImportMode);
                }
            }

            return _downloadedEpisodesImportService.ProcessPath(message.Path, message.ImportMode);
        }

        public void Execute(DownloadedAlbumsScanCommand message)
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
