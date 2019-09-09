using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDownloadedTracksImportService
    {
        List<ImportResult> ProcessRootFolder(IDirectoryInfo directoryInfo);
        List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Artist artist = null, DownloadClientItem downloadClientItem = null);
        bool ShouldDeleteFolder(IDirectoryInfo directoryInfo, Artist artist);
    }

    public class DownloadedTracksImportService : IDownloadedTracksImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IDiskScanService _diskScanService;
        private readonly IArtistService _artistService;
        private readonly IParsingService _parsingService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedTracks _importApprovedTracks;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DownloadedTracksImportService(IDiskProvider diskProvider,
                                             IDiskScanService diskScanService,
                                             IArtistService artistService,
                                             IParsingService parsingService,
                                             IMakeImportDecision importDecisionMaker,
                                             IImportApprovedTracks importApprovedTracks,
                                             IEventAggregator eventAggregator,
                                             Logger logger)
        {
            _diskProvider = diskProvider;
            _diskScanService = diskScanService;
            _artistService = artistService;
            _parsingService = parsingService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedTracks = importApprovedTracks;
            _eventAggregator = eventAggregator;
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

            foreach (var audioFile in _diskScanService.GetAudioFiles(directoryInfo.FullName, false))
            {
                var fileResults = ProcessFile(audioFile, ImportMode.Auto, null);
                results.AddRange(fileResults);
            }

            return results;
        }

        public List<ImportResult> ProcessPath(string path, ImportMode importMode = ImportMode.Auto, Artist artist = null, DownloadClientItem downloadClientItem = null)
        {
            if (_diskProvider.FolderExists(path))
            {
                var directoryInfo = _diskProvider.GetDirectoryInfo(path);

                if (artist == null)
                {
                    return ProcessFolder(directoryInfo, importMode, downloadClientItem);
                }

                return ProcessFolder(directoryInfo, importMode, artist, downloadClientItem);
            }

            if (_diskProvider.FileExists(path))
            {
                var fileInfo = _diskProvider.GetFileInfo(path);

                if (artist == null)
                {
                    return ProcessFile(fileInfo, importMode, downloadClientItem);
                }

                return ProcessFile(fileInfo, importMode, artist, downloadClientItem);
            }

            _logger.Error("Import failed, path does not exist or is not accessible by Lidarr: {0}", path);
            _eventAggregator.PublishEvent(new TrackImportFailedEvent(null, null, true, downloadClientItem));
            
            return new List<ImportResult>();
        }

        public bool ShouldDeleteFolder(IDirectoryInfo directoryInfo, Artist artist)
        {
            var audioFiles = _diskScanService.GetAudioFiles(directoryInfo.FullName);
            var rarFiles = _diskProvider.GetFiles(directoryInfo.FullName, SearchOption.AllDirectories).Where(f => Path.GetExtension(f).Equals(".rar", StringComparison.OrdinalIgnoreCase));

            foreach (var audioFile in audioFiles)
            {
                var albumParseResult = Parser.Parser.ParseMusicTitle(audioFile.Name);

                if (albumParseResult == null)
                {
                    _logger.Warn("Unable to parse file on import: [{0}]", audioFile);
                    return false;
                }

                _logger.Warn("Audio file detected: [{0}]", audioFile);
                return false;
            }

            if (rarFiles.Any(f => _diskProvider.GetFileSize(f) > 10.Megabytes()))
            {
                _logger.Warn("RAR file detected, will require manual cleanup");
                return false;
            }

            return true;
        }

        private List<ImportResult> ProcessFolder(IDirectoryInfo directoryInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
        {
            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var artist = _parsingService.GetArtist(cleanedUpName);

            if (artist == null)
            {
                _logger.Debug("Unknown Artist {0}", cleanedUpName);

                return new List<ImportResult>
                       {
                           UnknownArtistResult("Unknown Artist")
                       };
            }

            return ProcessFolder(directoryInfo, importMode, artist, downloadClientItem);
        }

        private List<ImportResult> ProcessFolder(IDirectoryInfo directoryInfo, ImportMode importMode, Artist artist, DownloadClientItem downloadClientItem)
        {
            if (_artistService.ArtistPathExists(directoryInfo.FullName))
            {
                _logger.Warn("Unable to process folder that is mapped to an existing artist");
                return new List<ImportResult>();
            }

            var cleanedUpName = GetCleanedUpFolderName(directoryInfo.Name);
            var folderInfo = Parser.Parser.ParseAlbumTitle(directoryInfo.Name);
            var trackInfo = new ParsedTrackInfo { };

            if (folderInfo != null)
            {
                _logger.Debug("{0} folder quality: {1}", cleanedUpName, folderInfo.Quality);

                trackInfo = new ParsedTrackInfo
                {
                    AlbumTitle = folderInfo.AlbumTitle,
                    ArtistTitle = folderInfo.ArtistName,
                    Quality = folderInfo.Quality,
                    ReleaseGroup = folderInfo.ReleaseGroup,
                    ReleaseHash = folderInfo.ReleaseHash,
                };
            }
            else
            {
                trackInfo = null;
            }

            var audioFiles = _diskScanService.FilterFiles(directoryInfo.FullName, _diskScanService.GetAudioFiles(directoryInfo.FullName));

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

            var decisions = _importDecisionMaker.GetImportDecisions(audioFiles, artist, trackInfo);
            var importResults = _importApprovedTracks.Import(decisions, true, downloadClientItem, importMode);

            if (importMode == ImportMode.Auto)
            {
                importMode = (downloadClientItem == null || downloadClientItem.CanMoveFiles) ? ImportMode.Move : ImportMode.Copy;
            }

            if (importMode == ImportMode.Move &&
                importResults.Any(i => i.Result == ImportResultType.Imported) &&
                ShouldDeleteFolder(directoryInfo, artist))
            {
                _logger.Debug("Deleting folder after importing valid files");
                _diskProvider.DeleteFolder(directoryInfo.FullName, true);
            }

            return importResults;
        }

        private List<ImportResult> ProcessFile(IFileInfo fileInfo, ImportMode importMode, DownloadClientItem downloadClientItem)
        {
            var artist = _parsingService.GetArtist(Path.GetFileNameWithoutExtension(fileInfo.Name));

            if (artist == null)
            {
                _logger.Debug("Unknown Artist for file: {0}", fileInfo.Name);

                return new List<ImportResult>
                       {
                           UnknownArtistResult(string.Format("Unknown Artist for file: {0}", fileInfo.Name), fileInfo.FullName)
                       };
            }

            return ProcessFile(fileInfo, importMode, artist, downloadClientItem);
        }

        private List<ImportResult> ProcessFile(IFileInfo fileInfo, ImportMode importMode, Artist artist, DownloadClientItem downloadClientItem)
        {
            if (Path.GetFileNameWithoutExtension(fileInfo.Name).StartsWith("._"))
            {
                _logger.Debug("[{0}] starts with '._', skipping", fileInfo.FullName);

                return new List<ImportResult>
                       {
                           new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = fileInfo.FullName }, new Rejection("Invalid music file, filename starts with '._'")), "Invalid music file, filename starts with '._'")
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

            var decisions = _importDecisionMaker.GetImportDecisions(new List<IFileInfo>() { fileInfo }, artist, null);

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
            return new ImportResult(new ImportDecision<LocalTrack>(new LocalTrack { Path = audioFile }, new Rejection("Locked file, try again later")), "Locked file, try again later");
        }

        private ImportResult UnknownArtistResult(string message, string audioFile = null)
        {
            var localTrack = audioFile == null ? null : new LocalTrack { Path = audioFile };

            return new ImportResult(new ImportDecision<LocalTrack>(localTrack, new Rejection("Unknown Artist")), message);
        }
    }
}
