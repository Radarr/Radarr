using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.MediaFiles.TrackImport;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles
{
    public interface IDiskScanService
    {
        void Scan(List<string> folders = null, FilterFilesType filter = FilterFilesType.Known, bool addNewArtists = false, List<int> artistIds = null);
        IFileInfo[] GetAudioFiles(string path, bool allDirectories = true);
        string[] GetNonAudioFiles(string path, bool allDirectories = true);
        List<IFileInfo> FilterFiles(string basePath, IEnumerable<IFileInfo> files);
        List<string> FilterFiles(string basePath, IEnumerable<string> files);
    }

    public class DiskScanService :
        IDiskScanService,
        IExecute<RescanFoldersCommand>
    {
        public static readonly Regex ExcludedSubFoldersRegex = new Regex(@"(?:\\|\/|^)(?:extras|@eadir|extrafanart|plex versions|\.[^\\/]+)(?:\\|\/)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        public static readonly Regex ExcludedFilesRegex = new Regex(@"^\._|^Thumbs\.db$|^\.DS_store$|\.partial~$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly IDiskProvider _diskProvider;
        private readonly IMediaFileService _mediaFileService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IImportApprovedTracks _importApprovedTracks;
        private readonly IArtistService _artistService;
        private readonly IMediaFileTableCleanupService _mediaFileTableCleanupService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public DiskScanService(IDiskProvider diskProvider,
                               IMediaFileService mediaFileService,
                               IMakeImportDecision importDecisionMaker,
                               IImportApprovedTracks importApprovedTracks,
                               IArtistService artistService,
                               IRootFolderService rootFolderService,
                               IMediaFileTableCleanupService mediaFileTableCleanupService,
                               IEventAggregator eventAggregator,
                               Logger logger)
        {
            _diskProvider = diskProvider;
            _mediaFileService = mediaFileService;
            _importDecisionMaker = importDecisionMaker;
            _importApprovedTracks = importApprovedTracks;
            _artistService = artistService;
            _mediaFileTableCleanupService = mediaFileTableCleanupService;
            _rootFolderService = rootFolderService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public void Scan(List<string> folders = null, FilterFilesType filter = FilterFilesType.Known, bool addNewArtists = false, List<int> artistIds = null)
        {
            if (folders == null)
            {
                folders = _rootFolderService.All().Select(x => x.Path).ToList();
            }

            if (artistIds == null)
            {
                artistIds = new List<int>();
            }

            var mediaFileList = new List<IFileInfo>();

            var musicFilesStopwatch = Stopwatch.StartNew();

            foreach (var folder in folders)
            {
                // We could be scanning a root folder or a subset of a root folder.  If it's a subset,
                // check if the root folder exists before cleaning.
                var rootFolder = _rootFolderService.GetBestRootFolder(folder);

                if (rootFolder == null)
                {
                    _logger.Error("Not scanning {0}, it's not a subdirectory of a defined root folder", folder);
                    return;
                }

                if (!_diskProvider.FolderExists(rootFolder.Path))
                {
                    _logger.Warn("Root folder {0} doesn't exist.", rootFolder.Path);

                    var skippedArtists = _artistService.GetArtists(artistIds);
                    skippedArtists.ForEach(x => _eventAggregator.PublishEvent(new ArtistScanSkippedEvent(x, ArtistScanSkippedReason.RootFolderDoesNotExist)));
                    return;
                }

                if (_diskProvider.GetDirectories(rootFolder.Path).Empty())
                {
                    _logger.Warn("Root folder {0} is empty.", rootFolder.Path);

                    var skippedArtists = _artistService.GetArtists(artistIds);
                    skippedArtists.ForEach(x => _eventAggregator.PublishEvent(new ArtistScanSkippedEvent(x, ArtistScanSkippedReason.RootFolderIsEmpty)));
                    return;
                }

                if (!_diskProvider.FolderExists(folder))
                {
                    _logger.Debug("Specified scan folder ({0}) doesn't exist.", folder);

                    CleanMediaFiles(folder, new List<string>());
                    continue;
                }

                _logger.ProgressInfo("Scanning {0}", folder);

                var files = FilterFiles(folder, GetAudioFiles(folder));

                if (!files.Any())
                {
                    _logger.Warn("Scan folder {0} is empty.", folder);
                    continue;
                }

                CleanMediaFiles(folder, files.Select(x => x.FullName).ToList());
                mediaFileList.AddRange(files);
            }

            musicFilesStopwatch.Stop();
            _logger.Trace("Finished getting track files for:\n{0} [{1}]", folders.ConcatToString("\n"), musicFilesStopwatch.Elapsed);

            var decisionsStopwatch = Stopwatch.StartNew();

            var config = new ImportDecisionMakerConfig
            {
                Filter = filter,
                IncludeExisting = true,
                AddNewArtists = addNewArtists
            };

            var decisions = _importDecisionMaker.GetImportDecisions(mediaFileList, null, null, config);

            decisionsStopwatch.Stop();
            _logger.Debug("Import decisions complete [{0}]", decisionsStopwatch.Elapsed);

            var importStopwatch = Stopwatch.StartNew();
            _importApprovedTracks.Import(decisions, false);

            // decisions may have been filtered to just new files.  Anything new and approved will have been inserted.
            // Now we need to make sure anything new but not approved gets inserted
            // Note that knownFiles will include anything imported just now
            var knownFiles = new List<TrackFile>();
            folders.ForEach(x => knownFiles.AddRange(_mediaFileService.GetFilesWithBasePath(x)));

            var newFiles = decisions
                .ExceptBy(x => x.Item.Path, knownFiles, x => x.Path, PathEqualityComparer.Instance)
                .Select(decision => new TrackFile
                {
                    Path = decision.Item.Path,
                    Size = decision.Item.Size,
                    Modified = decision.Item.Modified,
                    DateAdded = DateTime.UtcNow,
                    Quality = decision.Item.Quality,
                    MediaInfo = decision.Item.FileTrackInfo.MediaInfo
                })
                .ToList();
            _mediaFileService.AddMany(newFiles);

            _logger.Debug($"Inserted {newFiles.Count} new unmatched trackfiles");

            // finally update info on size/modified for existing files
            var updatedFiles = knownFiles
                .Join(decisions,
                      x => x.Path,
                      x => x.Item.Path,
                      (file, decision) => new
                      {
                          File = file,
                          Item = decision.Item
                      },
                      PathEqualityComparer.Instance)
                .Where(x => x.File.Size != x.Item.Size ||
                       Math.Abs((x.File.Modified - x.Item.Modified).TotalSeconds) > 1)
                .Select(x =>
                {
                    x.File.Size = x.Item.Size;
                    x.File.Modified = x.Item.Modified;
                    x.File.MediaInfo = x.Item.FileTrackInfo.MediaInfo;
                    x.File.Quality = x.Item.Quality;
                    return x.File;
                })
                .ToList();

            _mediaFileService.Update(updatedFiles);

            _logger.Debug($"Updated info for {updatedFiles.Count} known files");

            var artists = _artistService.GetArtists(artistIds);
            foreach (var artist in artists)
            {
                CompletedScanning(artist);
            }

            importStopwatch.Stop();
            _logger.Debug("Track import complete for:\n{0} [{1}]", folders.ConcatToString("\n"), importStopwatch.Elapsed);
        }

        private void CleanMediaFiles(string folder, List<string> mediaFileList)
        {
            _logger.Debug($"Cleaning up media files in DB [{folder}]");
            _mediaFileTableCleanupService.Clean(folder, mediaFileList);
        }

        private void CompletedScanning(Artist artist)
        {
            _logger.Info("Completed scanning disk for {0}", artist.Name);
            _eventAggregator.PublishEvent(new ArtistScannedEvent(artist));
        }

        public IFileInfo[] GetAudioFiles(string path, bool allDirectories = true)
        {
            _logger.Debug("Scanning '{0}' for music files", path);

            var searchOption = allDirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var filesOnDisk = _diskProvider.GetFileInfos(path, searchOption);

            var mediaFileList = filesOnDisk.Where(file => MediaFileExtensions.Extensions.Contains(file.Extension))
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

        public List<IFileInfo> FilterFiles(string basePath, IEnumerable<IFileInfo> files)
        {
            return files.Where(file => !ExcludedSubFoldersRegex.IsMatch(basePath.GetRelativePath(file.FullName)))
                        .Where(file => !ExcludedFilesRegex.IsMatch(file.Name))
                        .ToList();
        }

        public void Execute(RescanFoldersCommand message)
        {
            Scan(message.Folders, message.Filter, message.AddNewArtists, message.ArtistIds);
        }
    }
}
