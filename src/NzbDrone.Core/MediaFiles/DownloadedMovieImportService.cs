using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.MediaFiles.EpisodeImport;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Tv;
using NzbDrone.Core.Download;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.MediaFiles.Commands;

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
        private readonly IConfigService _config;
        private readonly Logger _logger;

        public DownloadedMovieImportService(IDiskProvider diskProvider,
                                               IDiskScanService diskScanService,
                                               IMovieService movieService,
                                               IParsingService parsingService,
                                               IMakeImportDecision importDecisionMaker,
                                               IImportApprovedMovie importApprovedMovie,
                                               IDetectSample detectSample,
                                               IConfigService config,
                                               Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _movieService = movieService;
            _parsingService = parsingService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedMovie = importApprovedMovie;
            _detectSample = detectSample;
            _config = config;
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

            _logger.Error("Import failed, path does not exist or is not accessible by Radarr: {0}", path);
            return new List<ImportResult>();
        }

        public bool ShouldDeleteFolder(DirectoryInfo directoryInfo, Movie movie)
        {
            var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);
            var rarFiles = _diskProvider.GetFiles(directoryInfo.FullName, SearchOption.AllDirectories).Where(f => Path.GetExtension(f) == ".rar");

            foreach (var videoFile in videoFiles)
            {
                var episodeParseResult = Parser.Parser.ParseTitle(Path.GetFileName(videoFile));

                if (episodeParseResult == null)
                {
                    _logger.Warn("Unable to parse file on import: [{0}]", videoFile);
                    return false;
                }

                var size = _diskProvider.GetFileSize(videoFile);
                var quality = QualityParser.ParseQuality(videoFile);

                if (!_detectSample.IsSample(movie, quality, videoFile, size, episodeParseResult.IsPossibleSpecialEpisode))
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
                _logger.Warn("Unable to process folder that is mapped to an existing show");
                return new List<ImportResult>();
            }

            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var folderInfo = Parser.Parser.ParseMovieTitle(directoryInfo.Name, _config.ParsingLeniency > 0);

            if (folderInfo != null)
            {
                _logger.Debug("{0} folder quality: {1}", cleanedUpName, folderInfo.Quality);
            }

            var videoFiles = _diskScanService.GetVideoFiles(directoryInfo.FullName);

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

            var decisions = _importDecisionMaker.GetImportDecisions(videoFiles.ToList(), movie, null, folderInfo, true, false);
            var importResults = _importApprovedMovie.Import(decisions, true, downloadClientItem, importMode);

            if ((downloadClientItem == null || !downloadClientItem.IsReadOnly) &&
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
                           new ImportResult(new ImportDecision(new LocalEpisode { Path = fileInfo.FullName }, new Rejection("Invalid video file, filename starts with '._'")), "Invalid video file, filename starts with '._'")
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

            var decisions = _importDecisionMaker.GetImportDecisions(new List<string>() { fileInfo.FullName }, movie, null, null, true, false);

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
            return new ImportResult(new ImportDecision(new LocalEpisode { Path = videoFile }, new Rejection("Locked file, try again later")), "Locked file, try again later");
        }

        private ImportResult UnknownMovieResult(string message, string videoFile = null)
        {
            var localEpisode = videoFile == null ? null : new LocalMovie { Path = videoFile };

            return new ImportResult(new ImportDecision(localEpisode, new Rejection("Unknown Movie")), message);
        }
    }
}
