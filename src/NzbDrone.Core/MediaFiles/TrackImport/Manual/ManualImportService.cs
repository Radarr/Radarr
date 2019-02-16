using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Music;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Cache;

namespace NzbDrone.Core.MediaFiles.TrackImport.Manual
{
    public interface IManualImportService
    {
        List<ManualImportItem> GetMediaFiles(string path, string downloadId, bool filterExistingFiles);
        void UpdateItems(List<ManualImportItem> item);
        ManualImportItem Find(int id);
    }

    public class ManualImportService : IExecute<ManualImportCommand>, IManualImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IParsingService _parsingService;
        private readonly IDiskScanService _diskScanService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IReleaseService _releaseService;
        private readonly ITrackService _trackService;
        private readonly IImportApprovedTracks _importApprovedTracks;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDownloadedTracksImportService _downloadedTracksImportService;
        private readonly ICached<ManualImportItem> _cache;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ManualImportService(IDiskProvider diskProvider,
                                   IParsingService parsingService,
                                   IDiskScanService diskScanService,
                                   IMakeImportDecision importDecisionMaker,
                                   IArtistService artistService,
                                   IAlbumService albumService,
                                   IReleaseService releaseService,
                                   ITrackService trackService,
                                   IImportApprovedTracks importApprovedTracks,
                                   ITrackedDownloadService trackedDownloadService,
                                   IDownloadedTracksImportService downloadedTracksImportService,
                                   ICacheManager cacheManager,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _diskScanService = diskScanService;
            _importDecisionMaker = importDecisionMaker;
            _artistService = artistService;
            _albumService = albumService;
            _releaseService = releaseService;
            _trackService = trackService;
            _importApprovedTracks = importApprovedTracks;
            _trackedDownloadService = trackedDownloadService;
            _downloadedTracksImportService = downloadedTracksImportService;
            _cache = cacheManager.GetCache<ManualImportItem>(GetType());
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public ManualImportItem Find(int id)
        {
            return _cache.Find(id.ToString());
        }

        public List<ManualImportItem> GetMediaFiles(string path, string downloadId, bool filterExistingFiles)
        {
            _cache.Clear();
            
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

                var decision = _importDecisionMaker.GetImportDecisions(new List<string> { path }, null, null, null, null, false, true, false);
                var result = MapItem(decision.First(), Path.GetDirectoryName(path), downloadId);
                _cache.Set(result.Id.ToString(), result);

                return new List<ManualImportItem> { result };
            }

            var items = ProcessFolder(path, downloadId, filterExistingFiles);
            foreach (var item in items)
            {
                _cache.Set(item.Id.ToString(), item);
            }
            
            return items;
        }

        private List<ManualImportItem> ProcessFolder(string folder, string downloadId, bool filterExistingFiles)
        {
            var directoryInfo = new DirectoryInfo(folder);
            var artist = _parsingService.GetArtist(directoryInfo.Name);

            if (artist == null && downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                artist = trackedDownload.RemoteAlbum.Artist;
            }

            var folderInfo = Parser.Parser.ParseMusicTitle(directoryInfo.Name);
            var artistFiles = _diskScanService.GetAudioFiles(folder).ToList();
            var decisions = _importDecisionMaker.GetImportDecisions(artistFiles, artist, null, null, folderInfo, filterExistingFiles, true, false);

            return decisions.Select(decision => MapItem(decision, folder, downloadId)).ToList();
        }

        public void UpdateItems(List<ManualImportItem> items)
        {
            var groupedItems = items.GroupBy(x => x.Album?.Id);
            _logger.Debug("UpdateItems, {0} groups", groupedItems.Count());
            foreach(var group in groupedItems)
            {
                // generate dummy decisions that don't match the release
                _logger.Debug("UpdateItems, group key: {0}", group.Key);
                var decisions = _importDecisionMaker.GetImportDecisions(group.Select(x => x.Path).ToList(), group.First().Artist, group.First().Album, null, null, false, true, true);

                foreach (var decision in decisions)
                {
                    var item = items.Where(x => x.Path == decision.Item.Path).Single();

                    if (decision.Item.Artist != null)
                    {
                        item.Artist = decision.Item.Artist;
                    }

                    if (decision.Item.Album != null)
                    {
                        item.Album = decision.Item.Album;
                        item.Release = decision.Item.Release;
                    }

                    if (decision.Item.Tracks.Any())
                    {
                        item.Tracks = decision.Item.Tracks;
                    }

                    item.Rejections = decision.Rejections;

                    _cache.Set(item.Id.ToString(), item);
                }
            }
        }

        private ManualImportItem MapItem(ImportDecision<LocalTrack> decision, string folder, string downloadId)
        {
            var item = new ManualImportItem();

            item.Id = HashConverter.GetHashInt31(decision.Item.Path);
            item.Path = decision.Item.Path;
            item.RelativePath = folder.GetRelativePath(decision.Item.Path);
            item.Name = Path.GetFileNameWithoutExtension(decision.Item.Path);
            item.DownloadId = downloadId;

            if (decision.Item.Artist != null)
            {
                item.Artist = decision.Item.Artist;
            }

            if (decision.Item.Album != null)
            {
                item.Album = decision.Item.Album;
                item.Release = decision.Item.Release;
            }

            if (decision.Item.Tracks.Any())
            {
                item.Tracks = decision.Item.Tracks;
            }

            item.Quality = decision.Item.Quality;
            item.Language = decision.Item.Language;
            item.Size = _diskProvider.GetFileSize(decision.Item.Path);
            item.Rejections = decision.Rejections;
            item.Tags = decision.Item.FileTrackInfo;

            return item;
        }

        public void Execute(ManualImportCommand message)
        {
            _logger.ProgressTrace("Manually importing {0} files using mode {1}", message.Files.Count, message.ImportMode);

            var imported = new List<ImportResult>();
            var importedTrackedDownload = new List<ManuallyImportedFile>();
            var albumIds = message.Files.GroupBy(e => e.AlbumId).ToList();
            var fileCount = 0;

            foreach (var importAlbumId in albumIds)
            {
                var albumImportDecisions = new List<ImportDecision<LocalTrack>>();

                foreach (var file in importAlbumId)
                {
                    _logger.ProgressTrace("Processing file {0} of {1}", fileCount + 1, message.Files.Count);

                    var artist = _artistService.GetArtist(file.ArtistId);
                    var album = _albumService.GetAlbum(file.AlbumId);
                    var release = _releaseService.GetRelease(file.AlbumReleaseId);
                    var tracks = _trackService.GetTracks(file.TrackIds);
                    var fileTrackInfo = Parser.Parser.ParseMusicPath(file.Path) ?? new ParsedTrackInfo();

                    var localTrack = new LocalTrack
                    {
                        ExistingFile = false,
                        Tracks = tracks,
                        MediaInfo = null,
                        FileTrackInfo = fileTrackInfo,
                        Path = file.Path,
                        Quality = file.Quality,
                        Language = file.Language,
                        Artist = artist,
                        Album = album,
                        Release = release,
                        Size = 0
                    };

                    albumImportDecisions.Add(new ImportDecision<LocalTrack>(localTrack));
                    fileCount += 1;
                }

                var existingFile = albumImportDecisions.First().Item.Artist.Path.IsParentPath(importAlbumId.First().Path);

                if (importAlbumId.First().DownloadId.IsNullOrWhiteSpace())
                {
                    imported.AddRange(_importApprovedTracks.Import(albumImportDecisions, !existingFile, null, message.ImportMode));
                }
                else
                {
                    var trackedDownload = _trackedDownloadService.Find(importAlbumId.First().DownloadId);
                    var importResults = _importApprovedTracks.Import(albumImportDecisions, true, trackedDownload.DownloadItem, message.ImportMode);

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
                            new DirectoryInfo(trackedDownload.DownloadItem.OutputPath.FullPath),
                            trackedDownload.RemoteAlbum.Artist) && trackedDownload.DownloadItem.CanMoveFiles)
                    {
                        _diskProvider.DeleteFolder(trackedDownload.DownloadItem.OutputPath.FullPath, true);
                    }
                }

                if (groupedTrackedDownload.Select(c => c.ImportResult).Count(c => c.Result == ImportResultType.Imported) >= Math.Max(1, trackedDownload.RemoteAlbum.Albums.Count))
                {
                    trackedDownload.State = TrackedDownloadStage.Imported;
                    _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                }
            }
        }
    }
}
