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
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.RootFolders;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.MediaFiles.TrackImport;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDiskScanService
    {
        void Scan(Artist artist);
        string[] GetAudioFiles(string path, bool allDirectories = true);
        string[] GetNonAudioFiles(string path, bool allDirectories = true);
        List<string> FilterFiles(string basePath, IEnumerable<string> files);
    }

    public class DiskScanService :
        IDiskScanService,
        IHandle<ArtistUpdatedEvent>,
        IExecute<RescanArtistCommand> 
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedTracks _importApprovedTracks;
        private readonly IConfigService _configService;
        private readonly IArtistService _artistService;
        private readonly IMediaFileTableCleanupService _mediaFileTableCleanupService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DiskScanService(IDiskProvider diskProvider,
                               IMakeImportDecision importDecisionMaker,
                               IImportApprovedTracks importApprovedTracks,
                               IConfigService configService,
                               IArtistService artistService,
                               IRootFolderService rootFolderService,
                               IMediaFileTableCleanupService mediaFileTableCleanupService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _diskProvider = diskProvider;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedTracks = importApprovedTracks;
            _configService = configService;
            _artistService = artistService;
            _mediaFileTableCleanupService = mediaFileTableCleanupService;
            _rootFolderService = rootFolderService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }
        private static readonly Regex ExcludedSubFoldersRegex = new Regex(@"(?:\\|\/|^)(?:extras|@eadir|extrafanart|plex versions|\.[^\\/]+)(?:\\|\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private static readonly Regex ExcludedFilesRegex = new Regex(@"^\._|^Thumbs\.db$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void Scan(Artist artist)
        {
            var rootFolder = _rootFolderService.GetBestRootFolderPath(artist.Path);

            if (!_diskProvider.FolderExists(rootFolder))
            {
                _logger.Warn("Artist' root folder ({0}) doesn't exist.", rootFolder);
                _eventAggregator.PublishEvent(new ArtistScanSkippedEvent(artist, ArtistScanSkippedReason.RootFolderDoesNotExist));
                return;
            }

            if (_diskProvider.GetDirectories(rootFolder).Empty())
            {
                _logger.Warn("Artist' root folder ({0}) is empty.", rootFolder);
                _eventAggregator.PublishEvent(new ArtistScanSkippedEvent(artist, ArtistScanSkippedReason.RootFolderIsEmpty));
                return;
            }

            _logger.ProgressInfo("Scanning {0}", artist.Name);

            if (!_diskProvider.FolderExists(artist.Path))
            {
                if (_configService.CreateEmptyArtistFolders)
                {
                    _logger.Debug("Creating missing artist folder: {0}", artist.Path);
                    _diskProvider.CreateFolder(artist.Path);
                    SetPermissions(artist.Path);
                }
                else
                {
                    _logger.Debug("Artist folder doesn't exist: {0}", artist.Path);
                }

                CleanMediaFiles(artist, new List<string>());
                CompletedScanning(artist);

                return;
            }

            var musicFilesStopwatch = Stopwatch.StartNew();
            var mediaFileList = FilterFiles(artist.Path, GetAudioFiles(artist.Path)).ToList();
            musicFilesStopwatch.Stop();
            _logger.Trace("Finished getting track files for: {0} [{1}]", artist, musicFilesStopwatch.Elapsed);

            CleanMediaFiles(artist, mediaFileList);

            var decisionsStopwatch = Stopwatch.StartNew();
            var decisions = _importDecisionMaker.GetImportDecisions(mediaFileList, artist);
            decisionsStopwatch.Stop();
            _logger.Trace("Import decisions complete for: {0} [{1}]", artist, decisionsStopwatch.Elapsed);
            _importApprovedTracks.Import(decisions, false);

            RemoveEmptyArtistFolder(artist.Path);
            CompletedScanning(artist);
        }
        
        private void CleanMediaFiles(Artist artist, List<string> mediaFileList)
        {
            _logger.Debug("{0} Cleaning up media files in DB", artist);
            _mediaFileTableCleanupService.Clean(artist, mediaFileList);
        }

        private void CompletedScanning(Artist artist)
        {
            _logger.Info("Completed scanning disk for {0}", artist.Name);
            _eventAggregator.PublishEvent(new ArtistScannedEvent(artist));
        }

        public string[] GetAudioFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for music files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption).ToList();

            var mediaFileList = filesOnDisk.Where(file => MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                           .ToList();

            _logger.Trace("{0} files were found in {1}", filesOnDisk.Count, path);
            _logger.Debug("{0} audio files were found in {1}", mediaFileList.Count, path);

            return mediaFileList.ToArray();
        }

        public string[] GetNonAudioFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for non-music files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFiles(path, searchOption).ToList();

            var mediaFileList = filesOnDisk.Where(file => !MediaFileExtensions.Extensions.Contains(Path.GetExtension(file)))
                                           .ToList();

            _logger.Trace("{0} files were found in {1}", filesOnDisk.Count, path);
            _logger.Debug("{0} non-music files were found in {1}", mediaFileList.Count, path);

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

        private void RemoveEmptyArtistFolder(string path)
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

        public void Handle(ArtistUpdatedEvent message)
        {
            Scan(message.Artist);
        }

        public void Execute(RescanArtistCommand message)
        {
            if (message.ArtistId.HasValue)
            {
                var artist = _artistService.GetArtist(message.ArtistId.Value);
                Scan(artist);
            }

            else
            {
                var allArtists = _artistService.GetAllArtists();

                foreach (var artist in allArtists)
                {
                    Scan(artist);
                }
            }
        }
    }
}
