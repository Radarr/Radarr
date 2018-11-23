using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Movies;

namespace NzbDrone.Core.MediaFiles.MovieImport.Manual
{
    public interface IManualImportService
    {
        List<ManualImportItem> GetMediaFiles(string path, string downloadId, bool filterExistingFiles);
    }

    public class ManualImportService : IExecute<ManualImportCommand>, IManualImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IParsingService _parsingService;
        private readonly IDiskScanService _diskScanService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IMovieService _movieService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IImportApprovedMovie _importApprovedMovie;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDownloadedMovieImportService _downloadedMovieImportService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _config;
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public ManualImportService(IDiskProvider diskProvider,
                                   IParsingService parsingService,
                                   IDiskScanService diskScanService,
                                   IMakeImportDecision importDecisionMaker,
                                   IMovieService movieService,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IImportApprovedMovie importApprovedMovie,
                                   ITrackedDownloadService trackedDownloadService,
                                   IDownloadedMovieImportService downloadedMovieImportService,
                                   IEventAggregator eventAggregator,
                                   IConfigService config,
                                   IHistoryService historyService,
                                   Logger logger)
        {
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _diskScanService = diskScanService;
            _importDecisionMaker = importDecisionMaker;
            _movieService = movieService;
            _videoFileInfoReader = videoFileInfoReader;
            _importApprovedMovie = importApprovedMovie;
            _trackedDownloadService = trackedDownloadService;
            _downloadedMovieImportService = downloadedMovieImportService;
            _eventAggregator = eventAggregator;
            _config = config;
            _historyService = historyService;
            _logger = logger;
        }

        public List<ManualImportItem> GetMediaFiles(string path, string downloadId, bool filterExistingFiles)
        {
            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);

                if (trackedDownload == null)
                {
                    return new List<ManualImportItem>();
                }

                path = trackedDownload.DownloadItem.OutputPath.FullPath;
            }

            if (!_diskProvider.FolderExists(path))
            {
                if (!_diskProvider.FileExists(path))
                {
                    return new List<ManualImportItem>();
                }

                return new List<ManualImportItem> { ProcessFile(path, downloadId) };
            }

            return ProcessFolder(path, downloadId, filterExistingFiles);
        }

        private List<ManualImportItem> ProcessFolder(string folder, string downloadId, bool filterExistingFiles)
        {
            DownloadClientItem downloadClientItem = null;
            var directoryInfo = new DirectoryInfo(folder);
            var movie = _parsingService.GetMovie(directoryInfo.Name);

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                downloadClientItem = trackedDownload.DownloadItem;

                if (movie == null)
                {
                    movie = trackedDownload.RemoteMovie.Movie;
                }
            }

            if (movie == null)
            {
                var files = _diskScanService.GetVideoFiles(folder);

                return files.Select(file => ProcessFile(file, downloadId, folder)).Where(i => i != null).ToList();
            }

            var historyItems = _historyService.FindByDownloadId(downloadId);
            var firstHistoryItem = historyItems.OrderByDescending(h => h.Date).FirstOrDefault();
            var folderInfo = _parsingService.ParseMovieInfo(directoryInfo.Name, new List<object>{firstHistoryItem});
            var movieFiles = _diskScanService.GetVideoFiles(folder).ToList();
            var decisions = _importDecisionMaker.GetImportDecisions(movieFiles, movie, downloadClientItem, folderInfo, SceneSource(movie, folder), false);

            return decisions.Select(decision => MapItem(decision, folder, downloadId)).ToList();
        }

        private ManualImportItem ProcessFile(string file, string downloadId, string folder = null)
        {
            if (folder.IsNullOrWhiteSpace())
            {
                folder = new FileInfo(file).Directory.FullName;
            }

            DownloadClientItem downloadClientItem = null;
            var relativeFile = folder.GetRelativePath(file);

            var movie = _parsingService.GetMovie(relativeFile.Split('\\', '/')[0]);

            if (movie == null)
            {
                movie = _parsingService.GetMovie(relativeFile);
            }

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                downloadClientItem = trackedDownload.DownloadItem;

                if (movie == null)
                {
                    movie = trackedDownload.RemoteMovie.Movie;
                }
            }

            if (movie == null)
            {
                var localMovie = new LocalMovie()
                {
                    Path = file,
                    Quality = QualityParser.ParseQuality(file),
                    Size = _diskProvider.GetFileSize(file)
                };

                return MapItem(new ImportDecision(localMovie, new Rejection("Unknown Movie")), folder, downloadId);
            }

            var importDecisions = _importDecisionMaker.GetImportDecisions(new List<string> { file },
                movie, downloadClientItem, null, SceneSource(movie, folder), true);

            return importDecisions.Any() ? MapItem(importDecisions.First(), folder, downloadId) : new ManualImportItem
                                                                                                  {
                                                                                                      DownloadId = downloadId,
                                                                                                      Path = file,
                                                                                                      RelativePath = folder.GetRelativePath(file),
                                                                                                      Name = Path.GetFileNameWithoutExtension(file),
                                                                                                      Rejections = new List<Rejection>
                                                                                                                   {
                                                                                                                       new Rejection("Unable to process file")
                                                                                                                   }
                                                                                                  };
        }

        private bool SceneSource(Movie movie, string folder)
        {
            return !(movie.Path.PathEquals(folder) || movie.Path.IsParentPath(folder));
        }

        private ManualImportItem MapItem(ImportDecision decision, string folder, string downloadId)
        {
            var item = new ManualImportItem();

            item.Path = decision.LocalMovie.Path;
            item.RelativePath = folder.GetRelativePath(decision.LocalMovie.Path);
            item.Name = Path.GetFileNameWithoutExtension(decision.LocalMovie.Path);
            item.DownloadId = downloadId;

            if (decision.LocalMovie.Movie != null)
            {
                item.Movie = decision.LocalMovie.Movie;
            }

            item.Quality = decision.LocalMovie.Quality;
            item.Size = _diskProvider.GetFileSize(decision.LocalMovie.Path);
            item.Rejections = decision.Rejections;

            return item;
        }

        public void Execute(ManualImportCommand message)
        {
            _logger.ProgressTrace("Manually importing {0} files using mode {1}", message.Files.Count, message.ImportMode);

            var imported = new List<ImportResult>();
            var importedTrackedDownload = new List<ManuallyImportedFile>();

            for (int i = 0; i < message.Files.Count; i++)
            {
                _logger.ProgressTrace("Processing file {0} of {1}", i + 1, message.Files.Count);

                var file = message.Files[i];
                var movie = _movieService.GetMovie(file.MovieId);
                var parsedMovieInfo = _parsingService.ParseMoviePathInfo(file.Path, new List<object>()) ?? new ParsedMovieInfo();
                var mediaInfo = _videoFileInfoReader.GetMediaInfo(file.Path);
                var existingFile = movie.Path.IsParentPath(file.Path);

                var localMovie = new LocalMovie
                {
                    ExistingFile = false,
                    MediaInfo = mediaInfo,
                    ParsedMovieInfo = parsedMovieInfo,
                    Path = file.Path,
                    Quality = file.Quality,
                    Movie = movie,
                    Size = 0
                };

                //TODO: Cleanup non-tracked downloads

                var importDecision = new ImportDecision(localMovie);

                if (file.DownloadId.IsNullOrWhiteSpace())
                {
                    imported.AddRange(_importApprovedMovie.Import(new List<ImportDecision> { importDecision }, !existingFile, null, message.ImportMode));
                }

                else
                {
                    var trackedDownload = _trackedDownloadService.Find(file.DownloadId);
                    var importResult = _importApprovedMovie.Import(new List<ImportDecision> { importDecision }, true, trackedDownload.DownloadItem, message.ImportMode).First();

                    imported.Add(importResult);

                    importedTrackedDownload.Add(new ManuallyImportedFile
                    {
                        TrackedDownload = trackedDownload,
                        ImportResult = importResult
                    });
                }
            }

            _logger.ProgressTrace("Manually imported {0} files", imported.Count);

            foreach (var groupedTrackedDownload in importedTrackedDownload.GroupBy(i => i.TrackedDownload.DownloadItem.DownloadId).ToList())
            {
                var trackedDownload = groupedTrackedDownload.First().TrackedDownload;

                if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath.FullPath))
                {
                    if (_downloadedMovieImportService.ShouldDeleteFolder(
                            new DirectoryInfo(trackedDownload.DownloadItem.OutputPath.FullPath),
                            trackedDownload.RemoteMovie.Movie) && trackedDownload.DownloadItem.CanMoveFiles)
                    {
                        _diskProvider.DeleteFolder(trackedDownload.DownloadItem.OutputPath.FullPath, true);
                    }
                }

                if (groupedTrackedDownload.Select(c => c.ImportResult).Count(c => c.Result == ImportResultType.Imported) >= Math.Max(1, 1)) //TODO: trackedDownload.RemoteMovie.Movie.Count is always 1?
                {
                    trackedDownload.State = TrackedDownloadStage.Imported;
                    _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                }
            }
        }
    }
}
