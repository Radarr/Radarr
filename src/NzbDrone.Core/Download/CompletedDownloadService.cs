using System;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.Download
{
    public interface ICompletedDownloadService
    {
        void Process(TrackedDownload trackedDownload, bool ignoreWarnings = false);
    }

    public class CompletedDownloadService : ICompletedDownloadService
    {
        private readonly IConfigService _configService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IHistoryService _historyService;
        private readonly IDownloadedMovieImportService _downloadedMovieImportService;
        private readonly IParsingService _parsingService;
        private readonly IMovieService _movieService;
        private readonly Logger _logger;

        public CompletedDownloadService(IConfigService configService,
                                        IEventAggregator eventAggregator,
                                        IHistoryService historyService,
                                        IDownloadedMovieImportService downloadedMovieImportService,
                                        IParsingService parsingService,
                                        IMovieService movieService,
                                        Logger logger)
        {
            _configService = configService;
            _eventAggregator = eventAggregator;
            _historyService = historyService;
            _downloadedMovieImportService = downloadedMovieImportService;
            _parsingService = parsingService;
            _movieService = movieService;
            _logger = logger;
        }

        public void Process(TrackedDownload trackedDownload, bool ignoreWarnings = false)
        {
            if (trackedDownload.DownloadItem.Status != DownloadItemStatus.Completed)
            {
                return;
            }

            if (!ignoreWarnings)
            {
                var historyItem = _historyService.MostRecentForDownloadId(trackedDownload.DownloadItem.DownloadId);

                if (historyItem == null && trackedDownload.DownloadItem.Category.IsNullOrWhiteSpace())
                {
                    trackedDownload.Warn("Download wasn't grabbed by Radarr and not in a category, Skipping.");
                    return;
                }

                var downloadItemOutputPath = trackedDownload.DownloadItem.OutputPath;

                if (downloadItemOutputPath.IsEmpty)
                {
                    trackedDownload.Warn("Download doesn't contain intermediate path, Skipping.");
                    return;
                }

                if ((OsInfo.IsWindows && !downloadItemOutputPath.IsWindowsPath) ||
                    (OsInfo.IsNotWindows && !downloadItemOutputPath.IsUnixPath))
                {
                    trackedDownload.Warn("[{0}] is not a valid local path. You may need a Remote Path Mapping.", downloadItemOutputPath);
                    return;
                }

                var movie = _parsingService.GetMovie(trackedDownload.DownloadItem.Title);
                if (movie == null)
                {
                    if (historyItem != null)
                    {
                        movie = _movieService.GetMovie(historyItem.MovieId);
                    }

                    if (movie == null)
                    {
                        trackedDownload.Warn("Movie title mismatch, automatic import is not possible.");
                        return;
                    }
                }
            }

            Import(trackedDownload);
        }

        private void Import(TrackedDownload trackedDownload)
        {
            var outputPath = trackedDownload.DownloadItem.OutputPath.FullPath;
            var importResults = _downloadedMovieImportService.ProcessPath(outputPath, ImportMode.Auto, trackedDownload.RemoteMovie.Movie, trackedDownload.DownloadItem);

            if (importResults.Empty())
            {
                trackedDownload.Warn("No files found are eligible for import in {0}", outputPath);
                return;
            }

            if (importResults.Count(c => c.Result == ImportResultType.Imported) >= 1)
            {
                trackedDownload.State = TrackedDownloadStage.Imported;
                _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                return;
            }

            if (importResults.Any(c => c.Result != ImportResultType.Imported))
            {
                var statusMessages = importResults
                    .Where(v => v.Result != ImportResultType.Imported)
                    .Select(v =>
                    {
                        if (v.ImportDecision.LocalMovie == null)
	                    {
                            return new TrackedDownloadStatusMessage("", v.Errors);
	                    }
                        return new TrackedDownloadStatusMessage(Path.GetFileName(v.ImportDecision.LocalMovie.Path), v.Errors);
                    })
                    .ToArray();

                trackedDownload.Warn(statusMessages);
            }
        }
    }
}
