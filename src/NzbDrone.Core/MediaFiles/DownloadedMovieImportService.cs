using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaFiles.MovieImport;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDownloadedMovieImportService
    {
        List<ImportResult> ProcessRootFolder(DirectoryInfo directoryInfo);
        List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Movie movie = null, DownloadClientItem downloadClientItem = null);
        bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Movie movie);
    }

    public class DownloadedMovieImportService : IDownloadedMovieImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly IMovieService _movieService;
        private readonly IParsingService _parsingService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedMovie _importApprovedMovie;
        private readonly IDetectSample _detectSample;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly IConfigService _config;
        private readonly IHistoryService _historyService;
        private readonly Logger _logger;

        public DownloadedMovieImportService(IDiskProvider diskProvider,
                                               IDiskScanService diskScanService,
                                               IMovieService movieService,
                                               IParsingService parsingService,
                                               IMakeImportDecision importDecisionMaker,
                                               IImportApprovedMovie importApprovedMovie,
                                               IDetectSample detectSample,
                                               IRuntimeInfo runtimeInfo,
                                               IConfigService config,
                                               IHistoryService historyService,
                                               Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _movieService = movieService;
            _parsingService = parsingService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedMovie = importApprovedMovie;
            _detectSample = detectSample;
            _runtimeInfo = runtimeInfo;
            _config = config;
            _historyService = historyService;
            _logger = logger;
        }

        public List<ImportResult> ProcessRootFolder(DirectoryInfo directoryInfo)
        {
            var results = new List<ImportResult>();

            foreach (var subFolder in _diskProvider.GetDirectories(directoryInfo.FullName))
            {
                var folderResults = ProcessFolder(new DirectoryInfo(subFolder), ImportMode.Auto, null);
                results.AddRange(folderResults);
            }

            foreach (var videoFile in _diskScanService.GetVideoFiles(directoryInfo.FullName, false))
            {
                var fileResults = ProcessFile(new FileInfo(videoFile), ImportMode.Auto, null);
                results.AddRange(fileResults);
            }

            return results;
        }

        public List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Movie movie = null, DownloadClientItem downloadClientItem = null)
        {
            _logger.Debug("Processing path: {0}", path);

            if (_diskProvider.FolderExists(path))
            {
                var directoryInfo = new DirectoryInfo(path);

                if (movie == null)
                {
                    return ProcessFolder(directoryInfo, importMode, downloadClientItem);
                }

                return ProcessFolder(directoryInfo, importMode, movie, downloadClientItem);
            }

            if (_diskProvider.FileExists(path))
            {
                var fileInfo = new FileInfo(path);

                if (movie == null)
                {
                    return ProcessFile(fileInfo, importMode, downloadClientItem);
                }

                return ProcessFile(fileInfo, importMode, movie, downloadClientItem);
            }

            LogInaccessiblePathError(path);
            return new List<ImportResult>();
        }

        public bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Movie movie)
        {
            try
                {
                var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);
                var rarFiles = _diskProvider.GetFiles(directoryInfo.FullName, SearchOption.AllDirectories)
                                            .Where(f => Path.GetExtension(f)
                                            .Equals(".rar", StringComparison.OrdinalIgnoreCase));

                foreach (var videoFile in videoFiles)
                {
                    var movieParseResult =
                        Parser.Parser.ParseMovieTitle(Path.GetFileName(videoFile));

                    if (movieParseResult == null)
                    {
                        _logger.Warn("Unable to parse file on import: [{0}]", videoFile);
                        return false;
                    }

                    if (_detectSample.IsSample(movie, videoFile) != DetectSampleResult.Sample)
                    {
                        _logger.Warn("Non-sample file detected: [{0}]", videoFile);
                        return false;
                    }
                }

                if (rarFiles.Any(f => _diskProvider.GetFileSize(f) > 10.Megabytes()))
                {
                    _logger.Warn("RAR file detected, will require manual cleanup");
                    return false;
                }

                return true;
            }
            catch (DirectoryNotFoundException e)
            {
                _logger.Debug(e, "Folder {0} has already been removed", directoryInfo.FullName);
                return false;
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Unable to determine whether folder {0} should be removed", directoryInfo.FullName);
                return false;
            }
        }

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
        {
            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var movie = _parsingService.GetMovie(cleanedUpName);

            if (movie == null)
            {
                _logger.Debug("Unknown Movie {0}", cleanedUpName);

                return new List<ImportResult>
                       {
                           UnknownMovieResult("Unknown Movie")
                       };
            }

            return ProcessFolder(directoryInfo, importMode, movie, downloadClientItem);
        }

        private List<ImportResult> ProcessFolder(DirectoryInfo directoryInfo, ImportMode importMode, Movie movie, DownloadClientItem downloadClientItem)
        {
            if (_movieService.MoviePathExists(directoryInfo.FullName))
            {
                _logger.Warn("Unable to process folder that is mapped to an existing movie");
                return new List<ImportResult>();
            }

            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var historyItems = _historyService.FindByDownloadId(downloadClientItem?.DownloadId ?? "");
            var firstHistoryItem = historyItems?.OrderByDescending(h => h.Date).FirstOrDefault();
            var folderInfo = _parsingService.ParseMovieInfo(cleanedUpName, new List<object> { firstHistoryItem });

            if (folderInfo != null)
            {
                _logger.Debug("{0} folder quality: {1}", cleanedUpName, folderInfo.Quality);
            }

            var videoFiles = _diskScanService.FilterPaths(directoryInfo.FullName, _diskScanService.GetVideoFiles(directoryInfo.FullName));

            if (downloadClientItem == null)
            {
                foreach (var videoFile in videoFiles)
                {
                    if (_diskProvider.IsFileLocked(videoFile))
                    {
                        return new List<ImportResult>
                               {
                                   FileIsLockedResult(videoFile)
                               };
                    }
                }
            }

            var decisions = _importDecisionMaker.GetImportDecisions(videoFiles.ToList(), movie, downloadClientItem, folderInfo, true);
            var importResults = _importApprovedMovie.Import(decisions, true, downloadClientItem, importMode);

            if (importMode == ImportMode.Auto)
            {
                importMode = (downloadClientItem == null || downloadClientItem.CanMoveFiles) ? ImportMode.Move : ImportMode.Copy;
            }

            if (importMode == ImportMode.Move &&
                importResults.Any(i => i.Result == ImportResultType.Imported) &&
                ShouldDeleteFolder(directoryInfo, movie))
            {
                _logger.Debug("Deleting folder after importing valid files");
                _diskProvider.DeleteFolder(directoryInfo.FullName, true);
            }

            return importResults;
        }

        private List<ImportResult> ProcessFile(FileInfo fileInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
        {
            var movie = _parsingService.GetMovie(Path.GetFileNameWithoutExtension(fileInfo.Name));

            if (movie == null)
            {
                _logger.Debug("Unknown Movie for file: {0}", fileInfo.Name);

                return new List<ImportResult>
                       {
                           UnknownMovieResult(string.Format("Unknown Movie for file: {0}", fileInfo.Name), fileInfo.FullName)
                       };
            }

            return ProcessFile(fileInfo, importMode, movie, downloadClientItem);
        }

        private List<ImportResult> ProcessFile(FileInfo fileInfo, ImportMode importMode, Movie movie, DownloadClientItem downloadClientItem)
        {
            if (Path.GetFileNameWithoutExtension(fileInfo.Name).StartsWith("._"))
            {
                _logger.Debug("[{0}] starts with '._', skipping", fileInfo.FullName);

                return new List<ImportResult>
                       {
                           new ImportResult(new ImportDecision(new LocalMovie { Path = fileInfo.FullName }, new Rejection("Invalid video file, filename starts with '._'")), "Invalid video file, filename starts with '._'")
                       };
            }

            if (downloadClientItem == null)
            {
                if (_diskProvider.IsFileLocked(fileInfo.FullName))
                {
                    return new List<ImportResult>
                           {
                               FileIsLockedResult(fileInfo.FullName)
                           };
                }
            }

            var decisions = _importDecisionMaker.GetImportDecisions(new List<string>() { fileInfo.FullName }, movie, downloadClientItem, null, true);

            return _importApprovedMovie.Import(decisions, true, downloadClientItem, importMode);
        }

        private string GetCleanedUpFolderName(string folder)
        {
            folder = folder.Replace("_UNPACK_", "")
                           .Replace("_FAILED_", "");

            return folder;
        }

        private ImportResult FileIsLockedResult(string videoFile)
        {
            _logger.Debug("[{0}] is currently locked by another process, skipping", videoFile);
            return new ImportResult(new ImportDecision(new LocalMovie { Path = videoFile }, new Rejection("Locked file, try again later")), "Locked file, try again later");
        }

        private ImportResult UnknownMovieResult(string message, string videoFile = null)
        {
            var localMovie = videoFile == null ? null : new LocalMovie { Path = videoFile };

            return new ImportResult(new ImportDecision(localMovie, new Rejection("Unknown Movie")), message);
        }

        private void LogInaccessiblePathError(string path)
        {
            if (_runtimeInfo.IsWindowsService)
            {
                var mounts = _diskProvider.GetMounts();
                var mount = mounts.FirstOrDefault(m => m.RootDirectory == Path.GetPathRoot(path));

                if (mount == null)
                {
                    _logger.Error("Import failed, path does not exist or is not accessible by Radarr: {0}. Unable to find a volume mounted for the path. If you're using a mapped network drive see the FAQ for more info", path);
                    return;
                }

                if (mount.DriveType == DriveType.Network)
                {
                    _logger.Error("Import failed, path does not exist or is not accessible by Radarr: {0}. It's recommended to avoid mapped network drives when running as a Windows service. See the FAQ for more info", path);
                    return;
                }
            }

            if (OsInfo.IsWindows)
            {
                if (path.StartsWith(@"\\"))
                {
                    _logger.Error("Import failed, path does not exist or is not accessible by Radarr: {0}. Ensure the user running Radarr has access to the network share", path);
                    return;
                }
            }

            _logger.Error("Import failed, path does not exist or is not accessible by Radarr: {0}. Ensure the path exists and the user running Radarr has the correct permissions to access this file/folder", path);
        }
    }
}
