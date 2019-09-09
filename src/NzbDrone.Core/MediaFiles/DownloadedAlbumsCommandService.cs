using System.Collections.Generic;
using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Common.Instrumentation.Extensions;

namespace NzbDrone.Core.MediaFiles
{
    public class DownloadedAlbumsCommandService : IExecute<DownloadedAlbumsScanCommand>
    {
        private readonly IDownloadedTracksImportService _downloadedTracksImportService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDiskProvider _diskProvider;
        private readonly Logger _logger;

        public DownloadedAlbumsCommandService(IDownloadedTracksImportService downloadedTracksImportService,
                                                ITrackedDownloadService trackedDownloadService,
                                                IDiskProvider diskProvider,
                                                Logger logger)
        {
            _downloadedTracksImportService = downloadedTracksImportService;
            _trackedDownloadService = trackedDownloadService;
            _diskProvider = diskProvider;
            _logger = logger;
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

                    return _downloadedTracksImportService.ProcessPath(message.Path, message.ImportMode, trackedDownload.RemoteAlbum.Artist, trackedDownload.DownloadItem);
                }
                else
                {
                    _logger.Warn("External directory scan request for unknown download {0}, attempting normal import. [{1}]", message.DownloadClientId, message.Path);

                    return _downloadedTracksImportService.ProcessPath(message.Path, message.ImportMode);
                }
            }

            return _downloadedTracksImportService.ProcessPath(message.Path, message.ImportMode);
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
                throw new ArgumentException("A path must be provided", "path");
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
