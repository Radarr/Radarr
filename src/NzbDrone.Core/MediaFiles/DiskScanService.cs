using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDiskScanService
    {
        void Scan(Movie movie);
        string[] GetVideoFiles(string path, bool allDirectories = true);
        string[] GetNonVideoFiles(string path, bool allDirectories = true);
        List<string> FilterFiles(string basePath, IEnumerable<string> files);
    }

    public class DiskScanService :
        IDiskScanService,
        IExecute<RescanMovieCommand>
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedMovie _importApprovedMovies;
        private readonly IConfigService _configService;
        private readonly IMovieService _movieService;
        private readonly IMediaFileTableCleanupService _mediaFileTableCleanupService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IMediaFileService _movieFileRepository;
        private readonly IRenameMovieFileService _renameMovieFiles;
        private readonly Logger _logger;

        public DiskScanService(IDiskProvider diskProvider,
                               IMakeImportDecision importDecisionMaker,
                               IImportApprovedMovie importApprovedMovies,
                               IConfigService configService,
                               IMovieService movieService,
                               IMediaFileTableCleanupService mediaFileTableCleanupService,
                               IRootFolderService rootFolderService,
                               IEventAggregator eventAggregator,
                               IMediaFileService movieFileRepository,
                               IRenameMovieFileService renameMovieFiles,
                               Logger logger)
        {
            _diskProvider = diskProvider;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedMovies = importApprovedMovies;
            _configService = configService;
            _movieService = movieService;
            _mediaFileTableCleanupService = mediaFileTableCleanupService;
            _rootFolderService = rootFolderService;
            _eventAggregator = eventAggregator;
            _movieFileRepository = movieFileRepository;
            _renameMovieFiles = renameMovieFiles;
            _logger = logger;
        }

        private static readonly Regex ExcludedSubFoldersRegex = new Regex(@"(?:\\|\/|^)(?:extras|@eadir|extrafanart|plex versions|\.[^\\/]+)(?:\\|\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedFilesRegex = new Regex(@"^\._|^Thumbs\.db$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Scan(Movie movie)
        {
            var rootFolder = _rootFolderService.GetBestRootFolderPath(movie.Path);

            if (!_diskProvider.FolderExists(rootFolder))
            {
                _logger.Warn("Movies' root folder ({0}) doesn't exist.", rootFolder);
                _eventAggregator.PublishEvent(new MovieScanSkippedEvent(movie, MovieScanSkippedReason.RootFolderDoesNotExist));
                return;
            }

            if (_diskProvider.GetDirectories(rootFolder).Empty())
            {
                _logger.Warn("Movies' root folder ({0}) is empty.", rootFolder);
                _eventAggregator.PublishEvent(new MovieScanSkippedEvent(movie, MovieScanSkippedReason.RootFolderIsEmpty));
                return;
            }

            _logger.ProgressInfo("Scanning disk for {0}", movie.Title);

            if (!_diskProvider.FolderExists(movie.Path))
            {
                if (_configService.CreateEmptyMovieFolders)
                {
                    _logger.Debug("Creating missing movie folder: {0}", movie.Path);
                    _diskProvider.CreateFolder(movie.Path);
                    SetPermissions(movie.Path);
                }
                else
                {
                    _logger.Debug("Movies folder doesn't exist: {0}", movie.Path);
                }

                CleanMediaFiles(movie, new List<string>());
                CompletedScanning(movie);

                return;
            }

            var videoFilesStopwatch = Stopwatch.StartNew();
            var mediaFileList = FilterFiles(movie.Path, GetVideoFiles(movie.Path)).ToList();
            videoFilesStopwatch.Stop();
            _logger.Trace("Finished getting movie files for: {0} [{1}]", movie, videoFilesStopwatch.Elapsed);

            CleanMediaFiles(movie, mediaFileList);

            var decisionsStopwatch = Stopwatch.StartNew();
            var decisions = _importDecisionMaker.GetImportDecisions(mediaFileList, movie);
            decisionsStopwatch.Stop();
            _logger.Trace("Import decisions complete for: {0} [{1}]", movie, decisionsStopwatch.Elapsed);
            _importApprovedMovies.Import(decisions, false);

            RemoveEmptyMovieFolder(movie.Path);
            CompletedScanning(movie);
        }

        private void CleanMediaFiles(Movie movie, List<string> mediaFileList)
        {
            _logger.Debug("{0} Cleaning up media files in DB", movie);
            _mediaFileTableCleanupService.Clean(movie, mediaFileList);
        }

        private void CompletedScanning(Movie movie)
        {
            _logger.Info("Completed scanning disk for {0}", movie.Title);
            _eventAggregator.PublishEvent(new MovieScannedEvent(movie));
        }

        public string[] GetVideoFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for video files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption).ToList();

            var mediaFileList = filesOnDisk.Where(file => MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                           .ToList();

            _logger.Trace("{0} files were found in {1}", filesOnDisk.Count, path);
            _logger.Debug("{0} video files were found in {1}", mediaFileList.Count, path);

            return mediaFileList.ToArray();
        }

        public string[] GetNonVideoFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for non-video files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption).ToList();

            var mediaFileList = filesOnDisk.Where(file => !MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                           .ToList();

            _logger.Trace("{0} files were found in {1}", filesOnDisk.Count, path);
            _logger.Debug("{0} non-video files were found in {1}", mediaFileList.Count, path);

            return mediaFileList.ToArray();
        }

        public List<string> FilterFiles(string basePath, IEnumerable<string> files)
        {
            return files.Where(file => !ExcludedSubFoldersRegex.IsMatch(basePath.GetRelativePath(file)))
                        .Where(file => !ExcludedFilesRegex.IsMatch(Path.GetFileName(file)))
                        .ToList();
        }

        private void SetPermissions(string path)
        {
            if (!_configService.SetPermissionsLinux)
            {
                return;
            }

            try
            {
                var permissions = _configService.FolderChmod;
                _diskProvider.SetPermissions(path, permissions, _configService.ChownUser, _configService.ChownGroup);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Unable to apply permissions to: " + path);
                _logger.Debug(ex, ex.Message);
            }
        }

        private void RemoveEmptyMovieFolder(string path)
        {
            if (_configService.DeleteEmptyFolders)
            {
                if (_diskProvider.GetFiles(path, SearchOption.AllDirectories).Empty())
                {
                    _diskProvider.DeleteFolder(path, true);
                }
                else
                {
                    _diskProvider.RemoveEmptySubfolders(path);
                }
            }
        }

        public void Execute(RescanMovieCommand message)
        {
            if (message.MovieId.HasValue)
            {
                var movie = _movieService.GetMovie(message.MovieId.Value);
                Scan(movie);
            }
            else
            {
                var allMovies = _movieService.GetAllMovies();

                foreach (var movie in allMovies)
                {
                    Scan(movie);
                }
            }
        }
    }
}
