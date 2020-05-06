using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles.TrackImport.Manual
{
    public interface IManualImportService
    {
        List<ManualImportItem> GetMediaFiles(string path, string downloadId, FilterFilesType filter, bool replaceExistingFiles);
        List<ManualImportItem> UpdateItems(List<ManualImportItem> item);
    }

    public class ManualImportService : IExecute<ManualImportCommand>, IManualImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IParsingService _parsingService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IDiskScanService _diskScanService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IAudioTagService _audioTagService;
        private readonly IImportApprovedTracks _importApprovedTracks;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDownloadedTracksImportService _downloadedTracksImportService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ManualImportService(IDiskProvider diskProvider,
                                   IParsingService parsingService,
                                   IRootFolderService rootFolderService,
                                   IDiskScanService diskScanService,
                                   IMakeImportDecision importDecisionMaker,
                                   IArtistService artistService,
                                   IAlbumService albumService,
                                   IAudioTagService audioTagService,
                                   IImportApprovedTracks importApprovedTracks,
                                   ITrackedDownloadService trackedDownloadService,
                                   IDownloadedTracksImportService downloadedTracksImportService,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _rootFolderService = rootFolderService;
            _diskScanService = diskScanService;
            _importDecisionMaker = importDecisionMaker;
            _artistService = artistService;
            _albumService = albumService;
            _audioTagService = audioTagService;
            _importApprovedTracks = importApprovedTracks;
            _trackedDownloadService = trackedDownloadService;
            _downloadedTracksImportService = downloadedTracksImportService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ManualImportItem> GetMediaFiles(string path, string downloadId, FilterFilesType filter, bool replaceExistingFiles)
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

                var files = new List<IFileInfo> { _diskProvider.GetFileInfo(path) };

                var config = new ImportDecisionMakerConfig
                {
                    Filter = FilterFilesType.None,
                    NewDownload = true,
                    SingleRelease = false,
                    IncludeExisting = !replaceExistingFiles,
                    AddNewArtists = false
                };

                var decision = _importDecisionMaker.GetImportDecisions(files, null, null, config);
                var result = MapItem(decision.First(), downloadId, replaceExistingFiles);

                return new List<ManualImportItem> { result };
            }

            return ProcessFolder(path, downloadId, filter, replaceExistingFiles);
        }

        private List<ManualImportItem> ProcessFolder(string folder, string downloadId, FilterFilesType filter, bool replaceExistingFiles)
        {
            DownloadClientItem downloadClientItem = null;
            var directoryInfo = new DirectoryInfo(folder);
            var artist = _parsingService.GetArtist(directoryInfo.Name);

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                downloadClientItem = trackedDownload.DownloadItem;

                if (artist == null)
                {
                    artist = trackedDownload.RemoteAlbum?.Artist;
                }
            }

            var artistFiles = _diskScanService.GetAudioFiles(folder).ToList();
            var idOverrides = new IdentificationOverrides
            {
                Artist = artist
            };
            var itemInfo = new ImportDecisionMakerInfo
            {
                DownloadClientItem = downloadClientItem,
                ParsedTrackInfo = Parser.Parser.ParseMusicTitle(directoryInfo.Name)
            };
            var config = new ImportDecisionMakerConfig
            {
                Filter = filter,
                NewDownload = true,
                SingleRelease = false,
                IncludeExisting = !replaceExistingFiles,
                AddNewArtists = false
            };

            var decisions = _importDecisionMaker.GetImportDecisions(artistFiles, idOverrides, itemInfo, config);

            // paths will be different for new and old files which is why we need to map separately
            var newFiles = artistFiles.Join(decisions,
                                            f => f.FullName,
                                            d => d.Item.Path,
                                            (f, d) => new { File = f, Decision = d },
                                            PathEqualityComparer.Instance);

            var newItems = newFiles.Select(x => MapItem(x.Decision, downloadId, replaceExistingFiles));
            var existingDecisions = decisions.Except(newFiles.Select(x => x.Decision));
            var existingItems = existingDecisions.Select(x => MapItem(x, null, replaceExistingFiles));

            return newItems.Concat(existingItems).ToList();
        }

        public List<ManualImportItem> UpdateItems(List<ManualImportItem> items)
        {
            var replaceExistingFiles = items.All(x => x.ReplaceExistingFiles);
            var groupedItems = items.Where(x => !x.AdditionalFile).GroupBy(x => x.Album?.Id);
            _logger.Debug($"UpdateItems, {groupedItems.Count()} groups, replaceExisting {replaceExistingFiles}");

            var result = new List<ManualImportItem>();

            foreach (var group in groupedItems)
            {
                _logger.Debug("UpdateItems, group key: {0}", group.Key);

                var files = group.Select(x => _diskProvider.GetFileInfo(x.Path)).ToList();
                var idOverride = new IdentificationOverrides
                {
                    Artist = group.First().Artist,
                    Album = group.First().Album,
                };
                var config = new ImportDecisionMakerConfig
                {
                    Filter = FilterFilesType.None,
                    NewDownload = true,
                    SingleRelease = true,
                    IncludeExisting = !replaceExistingFiles,
                    AddNewArtists = false
                };
                var decisions = _importDecisionMaker.GetImportDecisions(files, idOverride, null, config);

                var existingItems = group.Join(decisions,
                                               i => i.Path,
                                               d => d.Item.Path,
                                               (i, d) => new { Item = i, Decision = d },
                                               PathEqualityComparer.Instance);

                foreach (var pair in existingItems)
                {
                    var item = pair.Item;
                    var decision = pair.Decision;

                    if (decision.Item.Artist != null)
                    {
                        item.Artist = decision.Item.Artist;
                    }

                    if (decision.Item.Album != null)
                    {
                        item.Album = decision.Item.Album;
                    }

                    item.Rejections = decision.Rejections;

                    result.Add(item);
                }

                var newDecisions = decisions.Except(existingItems.Select(x => x.Decision));
                result.AddRange(newDecisions.Select(x => MapItem(x, null, replaceExistingFiles)));
            }

            return result;
        }

        private ManualImportItem MapItem(ImportDecision<LocalTrack> decision, string downloadId, bool replaceExistingFiles)
        {
            var item = new ManualImportItem();

            item.Id = HashConverter.GetHashInt31(decision.Item.Path);
            item.Path = decision.Item.Path;
            item.Name = Path.GetFileNameWithoutExtension(decision.Item.Path);
            item.DownloadId = downloadId;

            if (decision.Item.Artist != null)
            {
                item.Artist = decision.Item.Artist;
            }

            if (decision.Item.Album != null)
            {
                item.Album = decision.Item.Album;
            }

            item.Quality = decision.Item.Quality;
            item.Size = _diskProvider.GetFileSize(decision.Item.Path);
            item.Rejections = decision.Rejections;
            item.Tags = decision.Item.FileTrackInfo;
            item.AdditionalFile = decision.Item.AdditionalFile;
            item.ReplaceExistingFiles = replaceExistingFiles;

            return item;
        }

        public void Execute(ManualImportCommand message)
        {
            _logger.ProgressTrace("Manually importing {0} files using mode {1}", message.Files.Count, message.ImportMode);

            var imported = new List<ImportResult>();
            var importedTrackedDownload = new List<ManuallyImportedFile>();
            var bookIds = message.Files.GroupBy(e => e.BookId).ToList();
            var fileCount = 0;

            foreach (var importBookId in bookIds)
            {
                var albumImportDecisions = new List<ImportDecision<LocalTrack>>();

                foreach (var file in importBookId)
                {
                    _logger.ProgressTrace("Processing file {0} of {1}", fileCount + 1, message.Files.Count);

                    var artist = _artistService.GetArtist(file.AuthorId);
                    var album = _albumService.GetAlbum(file.BookId);
                    var fileTrackInfo = _audioTagService.ReadTags(file.Path) ?? new ParsedTrackInfo();
                    var fileInfo = _diskProvider.GetFileInfo(file.Path);

                    var localTrack = new LocalTrack
                    {
                        ExistingFile = artist.Path.IsParentPath(file.Path),
                        FileTrackInfo = fileTrackInfo,
                        Path = file.Path,
                        Size = fileInfo.Length,
                        Modified = fileInfo.LastWriteTimeUtc,
                        Quality = file.Quality,
                        Artist = artist,
                        Album = album
                    };

                    var importDecision = new ImportDecision<LocalTrack>(localTrack);
                    if (_rootFolderService.GetBestRootFolder(artist.Path) == null)
                    {
                        _logger.Warn($"Destination artist folder {artist.Path} not in a Root Folder, skipping import");
                        importDecision.Reject(new Rejection($"Destination artist folder {artist.Path} is not in a Root Folder"));
                    }

                    albumImportDecisions.Add(importDecision);
                    fileCount += 1;
                }

                var downloadId = importBookId.Select(x => x.DownloadId).FirstOrDefault(x => x.IsNotNullOrWhiteSpace());
                if (downloadId.IsNullOrWhiteSpace())
                {
                    imported.AddRange(_importApprovedTracks.Import(albumImportDecisions, message.ReplaceExistingFiles, null, message.ImportMode));
                }
                else
                {
                    var trackedDownload = _trackedDownloadService.Find(downloadId);
                    var importResults = _importApprovedTracks.Import(albumImportDecisions, message.ReplaceExistingFiles, trackedDownload.DownloadItem, message.ImportMode);

                    imported.AddRange(importResults);

                    foreach (var importResult in importResults)
                    {
                        importedTrackedDownload.Add(new ManuallyImportedFile
                        {
                            TrackedDownload = trackedDownload,
                            ImportResult = importResult
                        });
                    }
                }
            }

            _logger.ProgressTrace("Manually imported {0} files", imported.Count);

            foreach (var groupedTrackedDownload in importedTrackedDownload.GroupBy(i => i.TrackedDownload.DownloadItem.DownloadId).ToList())
            {
                var trackedDownload = groupedTrackedDownload.First().TrackedDownload;

                if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath.FullPath))
                {
                    if (_downloadedTracksImportService.ShouldDeleteFolder(
                            _diskProvider.GetDirectoryInfo(trackedDownload.DownloadItem.OutputPath.FullPath),
                            trackedDownload.RemoteAlbum.Artist) && trackedDownload.DownloadItem.CanMoveFiles)
                    {
                        _diskProvider.DeleteFolder(trackedDownload.DownloadItem.OutputPath.FullPath, true);
                    }
                }

                if (groupedTrackedDownload.Select(c => c.ImportResult).Count(c => c.Result == ImportResultType.Imported) >= Math.Max(1, trackedDownload.RemoteAlbum.Albums.Count))
                {
                    trackedDownload.State = TrackedDownloadState.Imported;
                    _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                }
            }
        }
    }
}
