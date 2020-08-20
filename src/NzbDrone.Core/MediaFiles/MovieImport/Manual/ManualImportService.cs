using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Manual
{
    public interface IManualImportService
    {
        List<ManualImportItem> GetMediaFiles(string path, string downloadId, int? movieId, bool filterExistingFiles);
        ManualImportItem ReprocessItem(string path, string downloadId, int movieId);
    }

    public class ManualImportService : IExecute<ManualImportCommand>, IManualImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IParsingService _parsingService;
        private readonly IDiskScanService _diskScanService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IMovieService _movieService;
        private readonly IImportApprovedMovie _importApprovedMovie;
        private readonly IAggregationService _aggregationService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDownloadedMovieImportService _downloadedMovieImportService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ManualImportService(IDiskProvider diskProvider,
                                   IParsingService parsingService,
                                   IDiskScanService diskScanService,
                                   IMakeImportDecision importDecisionMaker,
                                   IMovieService movieService,
                                   IAggregationService aggregationService,
                                   IImportApprovedMovie importApprovedMovie,
                                   ITrackedDownloadService trackedDownloadService,
                                   IDownloadedMovieImportService downloadedMovieImportService,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _diskScanService = diskScanService;
            _importDecisionMaker = importDecisionMaker;
            _movieService = movieService;
            _aggregationService = aggregationService;
            _importApprovedMovie = importApprovedMovie;
            _trackedDownloadService = trackedDownloadService;
            _downloadedMovieImportService = downloadedMovieImportService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ManualImportItem> GetMediaFiles(string path, string downloadId, int? movieId, bool filterExistingFiles)
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

                var rootFolder = Path.GetDirectoryName(path);
                return new List<ManualImportItem> { ProcessFile(rootFolder, rootFolder, path, downloadId) };
            }

            return ProcessFolder(path, path, downloadId, movieId, filterExistingFiles);
        }

        public ManualImportItem ReprocessItem(string path, string downloadId, int movieId)
        {
            var rootFolder = Path.GetDirectoryName(path);
            var movie = _movieService.GetMovie(movieId);

            return ProcessFile(rootFolder, rootFolder, path, downloadId, movie);
        }

        private List<ManualImportItem> ProcessFolder(string rootFolder, string baseFolder, string downloadId, int? movieId, bool filterExistingFiles)
        {
            DownloadClientItem downloadClientItem = null;
            var directoryInfo = new DirectoryInfo(baseFolder);

            var movie = movieId.HasValue ?
                _movieService.GetMovie(movieId.Value) :
                _parsingService.GetMovie(directoryInfo.Name);

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                downloadClientItem = trackedDownload.DownloadItem;

                if (movie == null)
                {
                    movie = trackedDownload.RemoteMovie?.Movie;
                }
            }

            if (movie == null)
            {
                var files = _diskScanService.FilterFiles(baseFolder, _diskScanService.GetVideoFiles(baseFolder, false));
                var subfolders = _diskScanService.FilterFiles(baseFolder, _diskProvider.GetDirectories(baseFolder));

                var processedFiles = files.Select(file => ProcessFile(rootFolder, baseFolder, file, downloadId));
                var processedFolders = subfolders.SelectMany(subfolder => ProcessFolder(rootFolder, subfolder, downloadId, null, filterExistingFiles));

                return processedFiles.Concat(processedFolders).Where(i => i != null).ToList();
            }

            var folderInfo = Parser.Parser.ParseMovieTitle(directoryInfo.Name);
            var movieFiles = _diskScanService.GetVideoFiles(baseFolder).ToList();
            var decisions = _importDecisionMaker.GetImportDecisions(movieFiles, movie, downloadClientItem, folderInfo, SceneSource(movie, baseFolder), filterExistingFiles);

            return decisions.Select(decision => MapItem(decision, rootFolder, downloadId, directoryInfo.Name)).ToList();
        }

        private ManualImportItem ProcessFile(string rootFolder, string baseFolder, string file, string downloadId, Movie movie = null)
        {
            DownloadClientItem downloadClientItem = null;
            var relativeFile = baseFolder.GetRelativePath(file);

            if (movie == null)
            {
                _parsingService.GetMovie(relativeFile.Split('\\', '/')[0]);
            }

            if (movie == null)
            {
                movie = _parsingService.GetMovie(relativeFile);
            }

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                downloadClientItem = trackedDownload?.DownloadItem;

                if (movie == null)
                {
                    movie = trackedDownload?.RemoteMovie?.Movie;
                }
            }

            if (movie == null)
            {
                var relativeParseInfo = Parser.Parser.ParseMoviePath(relativeFile);

                if (relativeParseInfo != null)
                {
                    movie = _movieService.FindByTitle(relativeParseInfo.MovieTitle, relativeParseInfo.Year);
                }
            }

            if (movie == null)
            {
                var localMovie = new LocalMovie();
                localMovie.Path = file;
                localMovie.Quality = QualityParser.ParseQuality(file);
                localMovie.Languages = LanguageParser.ParseLanguages(file);
                localMovie.Size = _diskProvider.GetFileSize(file);

                return MapItem(new ImportDecision(localMovie, new Rejection("Unknown Movie")), rootFolder, downloadId, null);
            }

            var importDecisions = _importDecisionMaker.GetImportDecisions(new List<string> { file }, movie, downloadClientItem, null, SceneSource(movie, baseFolder));

            if (importDecisions.Any())
            {
                return MapItem(importDecisions.First(), rootFolder, downloadId, null);
            }

            return new ManualImportItem
            {
                DownloadId = downloadId,
                Path = file,
                RelativePath = rootFolder.GetRelativePath(file),
                Name = Path.GetFileNameWithoutExtension(file),
                Rejections = new List<Rejection>()
            };
        }

        private bool SceneSource(Movie movie, string folder)
        {
            return !(movie.Path.PathEquals(folder) || movie.Path.IsParentPath(folder));
        }

        private ManualImportItem MapItem(ImportDecision decision, string rootFolder, string downloadId, string folderName)
        {
            var item = new ManualImportItem();

            item.Path = decision.LocalMovie.Path;
            item.FolderName = folderName;
            item.RelativePath = rootFolder.GetRelativePath(decision.LocalMovie.Path);
            item.Name = Path.GetFileNameWithoutExtension(decision.LocalMovie.Path);
            item.DownloadId = downloadId;

            if (decision.LocalMovie.Movie != null)
            {
                item.Movie = decision.LocalMovie.Movie;
            }

            item.Quality = decision.LocalMovie.Quality;
            item.Size = _diskProvider.GetFileSize(decision.LocalMovie.Path);
            item.Languages = decision.LocalMovie.Languages;
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
                var fileMovieInfo = Parser.Parser.ParseMoviePath(file.Path) ?? new ParsedMovieInfo();
                var existingFile = movie.Path.IsParentPath(file.Path);
                TrackedDownload trackedDownload = null;

                var localMovie = new LocalMovie
                {
                    ExistingFile = false,
                    FileMovieInfo = fileMovieInfo,
                    Path = file.Path,
                    Quality = file.Quality,
                    Languages = file.Languages,
                    Movie = movie,
                    Size = 0
                };

                if (file.DownloadId.IsNotNullOrWhiteSpace())
                {
                    trackedDownload = _trackedDownloadService.Find(file.DownloadId);
                    localMovie.DownloadClientMovieInfo = trackedDownload?.RemoteMovie?.ParsedMovieInfo;
                }

                if (file.FolderName.IsNotNullOrWhiteSpace())
                {
                    localMovie.FolderMovieInfo = Parser.Parser.ParseMovieTitle(file.FolderName);
                }

                localMovie = _aggregationService.Augment(localMovie, trackedDownload?.DownloadItem, false);

                // Apply the user-chosen values.
                localMovie.Movie = movie;
                localMovie.Quality = file.Quality;
                localMovie.Languages = file.Languages;

                //TODO: Cleanup non-tracked downloads
                var importDecision = new ImportDecision(localMovie);

                if (trackedDownload == null)
                {
                    imported.AddRange(_importApprovedMovie.Import(new List<ImportDecision> { importDecision }, !existingFile, null, message.ImportMode));
                }
                else
                {
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

                var importMovie = groupedTrackedDownload.First().ImportResult.ImportDecision.LocalMovie.Movie;

                if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath.FullPath))
                {
                    if (_downloadedMovieImportService.ShouldDeleteFolder(
                            new DirectoryInfo(trackedDownload.DownloadItem.OutputPath.FullPath),
                            importMovie) && trackedDownload.DownloadItem.CanMoveFiles)
                    {
                        _diskProvider.DeleteFolder(trackedDownload.DownloadItem.OutputPath.FullPath, true);
                    }
                }

                if (groupedTrackedDownload.Select(c => c.ImportResult).Count(c => c.Result == ImportResultType.Imported) >= 1)
                {
                    trackedDownload.State = TrackedDownloadState.Imported;
                    _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload, importMovie.Id));
                }
            }
        }
    }
}
