using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.CustomFormats;
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
        ManualImportItem ReprocessItem(string path, string downloadId, int movieId, string releaseGroup, QualityModel quality, List<Language> languages, int indexerFlags);
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
        private readonly ICustomFormatCalculationService _formatCalculator;
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
                                   ICustomFormatCalculationService formatCalculator,
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
            _formatCalculator = formatCalculator;
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

        public ManualImportItem ReprocessItem(string path, string downloadId, int movieId, string releaseGroup, QualityModel quality, List<Language> languages, int indexerFlags)
        {
            var rootFolder = Path.GetDirectoryName(path);
            var movie = _movieService.GetMovie(movieId);

            var downloadClientItem = GetTrackedDownload(downloadId)?.DownloadItem;

            var languageParse = LanguageParser.ParseLanguages(path);

            if (languageParse.Count <= 1 && languageParse.First() == Language.Unknown && movie != null)
            {
                languageParse = new List<Language> { movie.MovieMetadata.Value.OriginalLanguage };
                _logger.Debug("Language couldn't be parsed from release, fallback to movie original language: {0}", movie.MovieMetadata.Value.OriginalLanguage.Name);
            }

            var localMovie = new LocalMovie();
            localMovie.Movie = movie;
            localMovie.FileMovieInfo = Parser.Parser.ParseMoviePath(path);
            localMovie.DownloadClientMovieInfo = downloadClientItem == null ? null : Parser.Parser.ParseMovieTitle(downloadClientItem.Title);
            localMovie.DownloadItem = downloadClientItem;
            localMovie.Path = path;
            localMovie.SceneSource = SceneSource(movie, rootFolder);
            localMovie.ExistingFile = movie.Path.IsParentPath(path);
            localMovie.Size = _diskProvider.GetFileSize(path);
            localMovie.ReleaseGroup = releaseGroup.IsNullOrWhiteSpace() ? Parser.Parser.ParseReleaseGroup(path) : releaseGroup;
            localMovie.Languages = languages?.Count <= 1 && (languages?.SingleOrDefault() ?? Language.Unknown) == Language.Unknown ? languageParse : languages;
            localMovie.Quality = (quality?.Quality ?? Quality.Unknown) == Quality.Unknown ? QualityParser.ParseQuality(path) : quality;
            localMovie.IndexerFlags = (IndexerFlags)indexerFlags;
            localMovie.CustomFormats = _formatCalculator.ParseCustomFormat(localMovie);
            localMovie.CustomFormatScore = localMovie.Movie?.QualityProfile?.CalculateCustomFormatScore(localMovie.CustomFormats) ?? 0;

            return MapItem(_importDecisionMaker.GetDecision(localMovie, downloadClientItem), rootFolder, downloadId, null);
        }

        private List<ManualImportItem> ProcessFolder(string rootFolder, string baseFolder, string downloadId, int? movieId, bool filterExistingFiles)
        {
            DownloadClientItem downloadClientItem = null;
            Movie movie = null;

            var directoryInfo = new DirectoryInfo(baseFolder);

            if (movieId.HasValue)
            {
                movie = _movieService.GetMovie(movieId.Value);
            }
            else
            {
                try
                {
                    movie = _parsingService.GetMovie(directoryInfo.Name);
                }
                catch (MultipleMoviesFoundException e)
                {
                    _logger.Warn(e, "Unable to match movie by title");
                }
            }

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

                if (files.Count > 100)
                {
                    _logger.Warn("Unable to determine movie from folder name and found more than 100 files. Skipping parsing");

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
            try
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
                    localMovie.ReleaseGroup = Parser.Parser.ParseReleaseGroup(file);
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
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Failed to process file: {0}", file);
            }

            return new ManualImportItem
            {
                DownloadId = downloadId,
                Path = file,
                RelativePath = rootFolder.GetRelativePath(file),
                Name = Path.GetFileNameWithoutExtension(file),
                Size = _diskProvider.GetFileSize(file),
                Rejections = new List<Rejection>()
            };
        }

        private List<ManualImportItem> ProcessDownloadDirectory(string rootFolder, List<string> videoFiles)
        {
            var items = new List<ManualImportItem>();

            foreach (var file in videoFiles)
            {
                var localMovie = new LocalMovie();
                localMovie.Path = file;
                localMovie.Quality = new QualityModel(Quality.Unknown);
                localMovie.Languages = new List<Language> { Language.Unknown };
                localMovie.ReleaseGroup = Parser.Parser.ParseReleaseGroup(file);
                localMovie.Size = _diskProvider.GetFileSize(file);

                items.Add(MapItem(new ImportDecision(localMovie), rootFolder, null, null));
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

            item.Quality = decision.LocalMovie.Quality;
            item.Size = _diskProvider.GetFileSize(decision.LocalMovie.Path);
            item.Languages = decision.LocalMovie.Languages;
            item.ReleaseGroup = decision.LocalMovie.ReleaseGroup;
            item.Rejections = decision.Rejections;
            item.IndexerFlags = (int)decision.LocalMovie.IndexerFlags;

            if (decision.LocalMovie.Movie != null)
            {
                item.Movie = decision.LocalMovie.Movie;

                item.CustomFormats = _formatCalculator.ParseCustomFormat(decision.LocalMovie);
                item.CustomFormatScore = item.Movie.QualityProfile?.CalculateCustomFormatScore(item.CustomFormats) ?? 0;
            }

            return item;
        }

        public void Execute(ManualImportCommand message)
        {
            _logger.ProgressTrace("Manually importing {0} files using mode {1}", message.Files.Count, message.ImportMode);

            var imported = new List<ImportResult>();
            var importedTrackedDownload = new List<ManuallyImportedFile>();

            for (var i = 0; i < message.Files.Count; i++)
            {
                _logger.ProgressTrace("Processing file {0} of {1}", i + 1, message.Files.Count);

                var file = message.Files[i];
                var movie = _movieService.GetMovie(file.MovieId);
                var fileMovieInfo = Parser.Parser.ParseMoviePath(file.Path) ?? new ParsedMovieInfo();
                var existingFile = movie.Path.IsParentPath(file.Path);
                TrackedDownload trackedDownload = null;

                var localMovie = new LocalMovie
                {
                    ExistingFile = existingFile,
                    FileMovieInfo = fileMovieInfo,
                    Path = file.Path,
                    ReleaseGroup = file.ReleaseGroup,
                    Quality = file.Quality,
                    Languages = file.Languages,
                    IndexerFlags = (IndexerFlags)file.IndexerFlags,
                    Movie = movie,
                    Size = 0
                };

                if (file.DownloadId.IsNotNullOrWhiteSpace())
                {
                    trackedDownload = _trackedDownloadService.Find(file.DownloadId);
                    localMovie.DownloadClientMovieInfo = trackedDownload?.RemoteMovie?.ParsedMovieInfo;
                    localMovie.DownloadItem = trackedDownload?.DownloadItem;
                }

                if (file.FolderName.IsNotNullOrWhiteSpace())
                {
                    localMovie.FolderMovieInfo = Parser.Parser.ParseMovieTitle(file.FolderName);
                    localMovie.SceneSource = !existingFile;
                }

                // Augment movie file so imported files have all additional information an automatic import would
                localMovie = _aggregationService.Augment(localMovie, trackedDownload?.DownloadItem);

                // Apply the user-chosen values.
                localMovie.Movie = movie;
                localMovie.ReleaseGroup = file.ReleaseGroup;
                localMovie.Quality = file.Quality;
                localMovie.Languages = file.Languages;
                localMovie.IndexerFlags = (IndexerFlags)file.IndexerFlags;

                localMovie.CustomFormats = _formatCalculator.ParseCustomFormat(localMovie);
                localMovie.CustomFormatScore = localMovie.Movie.QualityProfile?.CalculateCustomFormatScore(localMovie.CustomFormats) ?? 0;

                // TODO: Cleanup non-tracked downloads
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
