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
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.MovieImport.Aggregation;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.MovieImport.Manual
{
    public interface IManualImportService
    {
        List<ManualImportItem> GetMediaFiles(string path, string downloadId, int? movieId, bool filterExistingFiles);
        ManualImportItem ReprocessItem(string path, string downloadId, int movieId, QualityModel quality, List<Language> languages);
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

                path = trackedDownload.ImportItem.OutputPath.FullPath;
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

        public ManualImportItem ReprocessItem(string path, string downloadId, int movieId, QualityModel quality, List<Language> languages)
        {
            var rootFolder = Path.GetDirectoryName(path);
            var movie = _movieService.GetMovie(movieId);

            var downloadClientItem = GetTrackedDownload(downloadId)?.DownloadItem;

            var localEpisode = new LocalMovie
            {
                Movie = movie,
                FileMovieInfo = Parser.Parser.ParseMoviePath(path),
                DownloadClientMovieInfo = downloadClientItem == null ? null : Parser.Parser.ParseMovieTitle(downloadClientItem.Title),
                Path = path,
                SceneSource = SceneSource(movie, rootFolder),
                ExistingFile = movie.Path.IsParentPath(path),
                Size = _diskProvider.GetFileSize(path),
                Languages = (languages?.SingleOrDefault() ?? Language.Unknown) == Language.Unknown ? LanguageParser.ParseLanguages(path) : languages,
                Quality = quality.Quality == Quality.Unknown ? QualityParser.ParseQuality(path) : quality
            };

            return MapItem(_importDecisionMaker.GetDecision(localEpisode, downloadClientItem), rootFolder, downloadId, null);
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
                // Filter paths based on the rootFolder, so files in subfolders that should be ignored are ignored.
                // It will lead to some extra directories being checked for files, but it saves the processing of them and is cleaner than
                // teaching FilterPaths to know whether it's processing a file or a folder and changing it's filtering based on that.
                // If the movie is unknown for the directory and there are more than 100 files in the folder don't process the items before returning.
                var files = _diskScanService.FilterPaths(rootFolder, _diskScanService.GetVideoFiles(baseFolder, false));

                if (files.Count() > 100)
                {
                    return ProcessDownloadDirectory(rootFolder, files);
                }

                var subfolders = _diskScanService.FilterPaths(rootFolder, _diskProvider.GetDirectories(baseFolder));

                var processedFiles = files.Select(file => ProcessFile(rootFolder, baseFolder, file, downloadId));
                var processedFolders = subfolders.SelectMany(subfolder => ProcessFolder(rootFolder, subfolder, downloadId, null, filterExistingFiles));

                return processedFiles.Concat(processedFolders).Where(i => i != null).ToList();
            }

            var folderInfo = Parser.Parser.ParseMovieTitle(directoryInfo.Name);
            var movieFiles = _diskScanService.FilterPaths(rootFolder, _diskScanService.GetVideoFiles(baseFolder).ToList());
            var decisions = _importDecisionMaker.GetImportDecisions(movieFiles, movie, downloadClientItem, folderInfo, SceneSource(movie, baseFolder), filterExistingFiles);

            return decisions.Select(decision => MapItem(decision, rootFolder, downloadId, directoryInfo.Name)).ToList();
        }

        private ManualImportItem ProcessFile(string rootFolder, string baseFolder, string file, string downloadId, Movie movie = null)
        {
            var trackedDownload = GetTrackedDownload(downloadId);
            var relativeFile = baseFolder.GetRelativePath(file);

            if (movie == null)
            {
                movie = _parsingService.GetMovie(relativeFile.Split('\\', '/')[0]);
            }

            if (movie == null)
            {
                movie = _parsingService.GetMovie(relativeFile);
            }

            if (trackedDownload != null && movie == null)
            {
                movie = trackedDownload?.RemoteMovie?.Movie;
            }

            if (movie == null)
            {
                var relativeParseInfo = Parser.Parser.ParseMoviePath(relativeFile);

                if (relativeParseInfo != null)
                {
                    movie = _movieService.FindByTitle(relativeParseInfo.PrimaryMovieTitle, relativeParseInfo.Year);
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

            var importDecisions = _importDecisionMaker.GetImportDecisions(new List<string> { file }, movie, trackedDownload?.DownloadItem, null, SceneSource(movie, baseFolder));

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

        private List<ManualImportItem> ProcessDownloadDirectory(string rootFolder, List<string> videoFiles)
        {
            var items = new List<ManualImportItem>();

            foreach (var file in videoFiles)
            {
                var localEpisode = new LocalMovie();
                localEpisode.Path = file;
                localEpisode.Quality = new QualityModel(Quality.Unknown);
                localEpisode.Languages = new List<Language> { Language.Unknown };
                localEpisode.Size = _diskProvider.GetFileSize(file);

                items.Add(MapItem(new ImportDecision(localEpisode), rootFolder, null, null));
            }

            return items;
        }

        private bool SceneSource(Movie movie, string folder)
        {
            return !(movie.Path.PathEquals(folder) || movie.Path.IsParentPath(folder));
        }

        private TrackedDownload GetTrackedDownload(string downloadId)
        {
            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);

                return trackedDownload;
            }

            return null;
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
                    localMovie.SceneSource = !existingFile;
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
                var outputPath = trackedDownload.ImportItem.OutputPath.FullPath;

                if (_diskProvider.FolderExists(outputPath))
                {
                    if (_downloadedMovieImportService.ShouldDeleteFolder(
                            new DirectoryInfo(outputPath),
                            importMovie) && trackedDownload.DownloadItem.CanMoveFiles)
                    {
                        _diskProvider.DeleteFolder(outputPath, true);
                    }
                }

                if (groupedTrackedDownload.Select(c => c.ImportResult).Any(c => c.Result == ImportResultType.Imported))
                {
                    trackedDownload.State = TrackedDownloadState.Imported;
                    _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload, importMovie.Id));
                }
            }
        }
    }
}
