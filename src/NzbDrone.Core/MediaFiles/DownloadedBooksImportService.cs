using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.BookImport;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDownloadedBooksImportService
    {
        List<ImportResult> ProcessRootFolder(IDirectoryInfo directoryInfo);
        List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Author author = null, DownloadClientItem downloadClientItem = null);
        bool ShouldDeleteFolder(IDirectoryInfo directoryInfo, Author author);
    }

    public class DownloadedBooksImportService : IDownloadedBooksImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly IAuthorService _authorService;
        private readonly IParsingService _parsingService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedBooks _importApprovedTracks;
        private readonly IEventAggregator _eventAggregator;
        private readonly IRuntimeInfo _runtimeInfo;
        private readonly Logger _logger;

        public DownloadedBooksImportService(IDiskProvider diskProvider,
                                             IDiskScanService diskScanService,
                                             IAuthorService authorService,
                                             IParsingService parsingService,
                                             IMakeImportDecision importDecisionMaker,
                                             IImportApprovedBooks importApprovedTracks,
                                             IEventAggregator eventAggregator,
                                             IRuntimeInfo runtimeInfo,
                                             Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _authorService = authorService;
            _parsingService = parsingService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedTracks = importApprovedTracks;
            _eventAggregator = eventAggregator;
            _runtimeInfo = runtimeInfo;
            _logger = logger;
        }

        public List<ImportResult> ProcessRootFolder(IDirectoryInfo directoryInfo)
        {
            var results = new List<ImportResult>();

            foreach (var subFolder in _diskProvider.GetDirectoryInfos(directoryInfo.FullName))
            {
                var folderResults = ProcessFolder(subFolder, ImportMode.Auto, null);
                results.AddRange(folderResults);
            }

            foreach (var audioFile in _diskScanService.GetBookFiles(directoryInfo.FullName, false))
            {
                var fileResults = ProcessFile(audioFile, ImportMode.Auto, null);
                results.AddRange(fileResults);
            }

            return results;
        }

        public List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Author author = null, DownloadClientItem downloadClientItem = null)
        {
            _logger.Debug("Processing path: {0}", path);

            if (_diskProvider.FolderExists(path))
            {
                var directoryInfo = _diskProvider.GetDirectoryInfo(path);

                if (author == null)
                {
                    return ProcessFolder(directoryInfo, importMode, downloadClientItem);
                }

                return ProcessFolder(directoryInfo, importMode, author, downloadClientItem);
            }

            if (_diskProvider.FileExists(path))
            {
                var fileInfo = _diskProvider.GetFileInfo(path);

                if (author == null)
                {
                    return ProcessFile(fileInfo, importMode, downloadClientItem);
                }

                return ProcessFile(fileInfo, importMode, author, downloadClientItem);
            }

            LogInaccessiblePathError(path);
            _eventAggregator.PublishEvent(new TrackImportFailedEvent(null, null, true, downloadClientItem));

            return new List<ImportResult>();
        }

        public bool ShouldDeleteFolder(IDirectoryInfo directoryInfo, Author author)
        {
            try
            {
                var bookFiles = _diskScanService.GetBookFiles(directoryInfo.FullName);
                var rarFiles = _diskProvider.GetFiles(directoryInfo.FullName, SearchOption.AllDirectories).Where(f => Path.GetExtension(f).Equals(".rar", StringComparison.OrdinalIgnoreCase));

                foreach (var bookFile in bookFiles)
                {
                    var bookParseResult = Parser.Parser.ParseTitle(bookFile.Name);

                    if (bookParseResult == null)
                    {
                        _logger.Warn("Unable to parse file on import: [{0}]", bookFile);
                        return false;
                    }

                    _logger.Warn("Book file detected: [{0}]", bookFile);
                    return false;
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
        }

        private List<ImportResult> ProcessFolder(IDirectoryInfo directoryInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
        {
            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var author = _parsingService.GetAuthor(cleanedUpName);

            if (author == null)
            {
                _logger.Debug("Unknown Author {0}", cleanedUpName);

                return new List<ImportResult>
                       {
                           UnknownAuthorResult("Unknown Author")
                       };
            }

            return ProcessFolder(directoryInfo, importMode, author, downloadClientItem);
        }

        private List<ImportResult> ProcessFolder(IDirectoryInfo directoryInfo, ImportMode importMode, Author author, DownloadClientItem downloadClientItem)
        {
            if (_authorService.AuthorPathExists(directoryInfo.FullName))
            {
                _logger.Warn("Unable to process folder that is mapped to an existing author");
                return new List<ImportResult>();
            }

            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var folderInfo = Parser.Parser.ParseBookTitle(directoryInfo.Name);
            var trackInfo = new ParsedTrackInfo { };

            if (folderInfo != null)
            {
                _logger.Debug("{0} folder quality: {1}", cleanedUpName, folderInfo.Quality);

                trackInfo = new ParsedTrackInfo
                {
                    BookTitle = folderInfo.BookTitle,
                    AuthorTitle = folderInfo.AuthorName,
                    Quality = folderInfo.Quality,
                    ReleaseGroup = folderInfo.ReleaseGroup,
                    ReleaseHash = folderInfo.ReleaseHash,
                };
            }
            else
            {
                trackInfo = null;
            }

            var audioFiles = _diskScanService.FilterFiles(directoryInfo.FullName, _diskScanService.GetBookFiles(directoryInfo.FullName));

            if (downloadClientItem == null)
            {
                foreach (var audioFile in audioFiles)
                {
                    if (_diskProvider.IsFileLocked(audioFile.FullName))
                    {
                        return new List<ImportResult>
                               {
                                   FileIsLockedResult(audioFile.FullName)
                               };
                    }
                }
            }

            var idOverrides = new IdentificationOverrides
            {
                Author = author
            };
            var idInfo = new ImportDecisionMakerInfo
            {
                DownloadClientItem = downloadClientItem,
                ParsedTrackInfo = trackInfo
            };
            var idConfig = new ImportDecisionMakerConfig
            {
                Filter = FilterFilesType.None,
                NewDownload = true,
                SingleRelease = false,
                IncludeExisting = false,
                AddNewAuthors = false
            };

            var decisions = _importDecisionMaker.GetImportDecisions(audioFiles, idOverrides, idInfo, idConfig);
            var importResults = _importApprovedTracks.Import(decisions, true, downloadClientItem, importMode);

            if (importMode == ImportMode.Auto)
            {
                importMode = (downloadClientItem == null || downloadClientItem.CanMoveFiles) ? ImportMode.Move : ImportMode.Copy;
            }

            if (importMode == ImportMode.Move &&
                importResults.Any(i => i.Result == ImportResultType.Imported) &&
                ShouldDeleteFolder(directoryInfo, author))
            {
                _logger.Debug("Deleting folder after importing valid files");
                _diskProvider.DeleteFolder(directoryInfo.FullName, true);
            }

            return importResults;
        }

        private List<ImportResult> ProcessFile(IFileInfo fileInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
        {
            var author = _parsingService.GetAuthor(Path.GetFileNameWithoutExtension(fileInfo.Name));

            if (author == null)
            {
                _logger.Debug("Unknown Author for file: {0}", fileInfo.Name);

                return new List<ImportResult>
                       {
                           UnknownAuthorResult(string.Format("Unknown Author for file: {0}", fileInfo.Name), fileInfo.FullName)
                       };
            }

            return ProcessFile(fileInfo, importMode, author, downloadClientItem);
        }

        private List<ImportResult> ProcessFile(IFileInfo fileInfo, ImportMode importMode, Author author, DownloadClientItem downloadClientItem)
        {
            if (Path.GetFileNameWithoutExtension(fileInfo.Name).StartsWith("._"))
            {
                _logger.Debug("[{0}] starts with '._', skipping", fileInfo.FullName);

                return new List<ImportResult>
                       {
                           new ImportResult(new ImportDecision<LocalBook>(new LocalBook { Path = fileInfo.FullName }, new Rejection("Invalid music file, filename starts with '._'")), "Invalid music file, filename starts with '._'")
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

            var idOverrides = new IdentificationOverrides
            {
                Author = author
            };
            var idInfo = new ImportDecisionMakerInfo
            {
                DownloadClientItem = downloadClientItem
            };
            var idConfig = new ImportDecisionMakerConfig
            {
                Filter = FilterFilesType.None,
                NewDownload = true,
                SingleRelease = false,
                IncludeExisting = false,
                AddNewAuthors = false
            };

            var decisions = _importDecisionMaker.GetImportDecisions(new List<IFileInfo>() { fileInfo }, idOverrides, idInfo, idConfig);

            return _importApprovedTracks.Import(decisions, true, downloadClientItem, importMode);
        }

        private string GetCleanedUpFolderName(string folder)
        {
            folder = folder.Replace("_UNPACK_", "")
                           .Replace("_FAILED_", "");

            return folder;
        }

        private ImportResult FileIsLockedResult(string audioFile)
        {
            _logger.Debug("[{0}] is currently locked by another process, skipping", audioFile);
            return new ImportResult(new ImportDecision<LocalBook>(new LocalBook { Path = audioFile }, new Rejection("Locked file, try again later")), "Locked file, try again later");
        }

        private ImportResult UnknownAuthorResult(string message, string bookFile = null)
        {
            var localTrack = bookFile == null ? null : new LocalBook { Path = bookFile };

            return new ImportResult(new ImportDecision<LocalBook>(localTrack, new Rejection("Unknown Author")), message);
        }

        private void LogInaccessiblePathError(string path)
        {
            if (_runtimeInfo.IsWindowsService)
            {
                var mounts = _diskProvider.GetMounts();
                var mount = mounts.FirstOrDefault(m => m.RootDirectory == Path.GetPathRoot(path));

                if (mount == null)
                {
                    _logger.Error("Import failed, path does not exist or is not accessible by Readarr: {0}. Unable to find a volume mounted for the path. If you're using a mapped network drive see the FAQ for more info", path);
                    return;
                }

                if (mount.DriveType == DriveType.Network)
                {
                    _logger.Error("Import failed, path does not exist or is not accessible by Readarr: {0}. It's recommended to avoid mapped network drives when running as a Windows service. See the FAQ for more info", path);
                    return;
                }
            }

            if (OsInfo.IsWindows)
            {
                if (path.StartsWith(@"\\"))
                {
                    _logger.Error("Import failed, path does not exist or is not accessible by Readarr: {0}. Ensure the user running Readarr has access to the network share", path);
                    return;
                }
            }

            _logger.Error("Import failed, path does not exist or is not accessible by Readarr: {0}. Ensure the path exists and the user running Readarr has the correct permissions to access this file/folder", path);
        }
    }
}
