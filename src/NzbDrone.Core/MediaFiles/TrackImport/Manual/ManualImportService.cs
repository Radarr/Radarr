using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.MediaFiles.MediaInfo;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.MediaFiles.TrackImport.Manual
{
    public interface IManualImportService
    {
        List<ManualImportItem> GetMediaFiles(string path, string downloadId, bool filterExistingFiles);
    }

    public class ManualImportService : IExecute<ManualImportCommand>, IManualImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IParsingService _parsingService;
        private readonly IDiskScanService _diskScanService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly ITrackService _trackService;
        private readonly IVideoFileInfoReader _videoFileInfoReader;
        private readonly IImportApprovedTracks _importApprovedTracks;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDownloadedTracksImportService _downloadedEpisodesImportService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ManualImportService(IDiskProvider diskProvider,
                                   IParsingService parsingService,
                                   IDiskScanService diskScanService,
                                   IMakeImportDecision importDecisionMaker,
                                   IArtistService artistService,
                                   IAlbumService albumService,
                                   ITrackService trackService,
                                   IVideoFileInfoReader videoFileInfoReader,
                                   IImportApprovedTracks importApprovedTracks,
                                   ITrackedDownloadService trackedDownloadService,
                                   IDownloadedTracksImportService downloadedEpisodesImportService,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _diskScanService = diskScanService;
            _importDecisionMaker = importDecisionMaker;
            _artistService = artistService;
            _albumService = albumService;
            _trackService = trackService;
            _videoFileInfoReader = videoFileInfoReader;
            _importApprovedTracks = importApprovedTracks;
            _trackedDownloadService = trackedDownloadService;
            _downloadedEpisodesImportService = downloadedEpisodesImportService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ManualImportItem> GetMediaFiles(string path, string downloadId, bool filterExistingFiles)
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

                return new List<ManualImportItem> { ProcessFile(path, downloadId) };
            }

            return ProcessFolder(path, downloadId, filterExistingFiles);
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

            if (artist == null)
            {
                var files = _diskScanService.FilterFiles(folder, _diskScanService.GetAudioFiles(folder));

                return files.Select(file => ProcessFile(file, downloadId, folder)).Where(i => i != null).ToList();
            }

            var folderInfo = Parser.Parser.ParseMusicTitle(directoryInfo.Name);
            var artistFiles = _diskScanService.GetAudioFiles(folder).ToList();
            var decisions = _importDecisionMaker.GetImportDecisions(artistFiles, artist, folderInfo, filterExistingFiles);

            return decisions.Select(decision => MapItem(decision, folder, downloadId)).ToList();
        }

        private ManualImportItem ProcessFile(string file, string downloadId, string folder = null)
        {
            if (folder.IsNullOrWhiteSpace())
            {
                folder = new FileInfo(file).Directory.FullName;
            }

            var relativeFile = folder.GetRelativePath(file);

            var artist = _parsingService.GetArtist(relativeFile.Split('\\', '/')[0]);

            if (artist == null)
            {
                artist = _parsingService.GetArtistFromTag(file);
            }

            if (artist == null && downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                artist = trackedDownload.RemoteAlbum.Artist;
            }

            if (artist == null)
            {
                var localTrack = new LocalTrack();
                localTrack.Path = file;
                localTrack.Quality = QualityParser.ParseQuality(file, null, 0);
                localTrack.Language = LanguageParser.ParseLanguage(file);
                localTrack.Size = _diskProvider.GetFileSize(file);

                return MapItem(new ImportDecision(localTrack, new Rejection("Unknown Artist")), folder, downloadId);
            }

            var importDecisions = _importDecisionMaker.GetImportDecisions(new List<string> { file },
                artist, null);

            return importDecisions.Any() ? MapItem(importDecisions.First(), folder, downloadId) : new ManualImportItem
            {
                DownloadId = downloadId,
                Path = file,
                RelativePath = folder.GetRelativePath(file),
                Name = Path.GetFileNameWithoutExtension(file),
                Rejections = new List<Rejection>
                {
                    new Rejection("Unable to process file")
                }
            };
        }

        private bool SceneSource(Artist artist, string folder)
        {
            return !(artist.Path.PathEquals(folder) || artist.Path.IsParentPath(folder));
        }

        private ManualImportItem MapItem(ImportDecision decision, string folder, string downloadId)
        {
            var item = new ManualImportItem();

            item.Path = decision.LocalTrack.Path;
            item.RelativePath = folder.GetRelativePath(decision.LocalTrack.Path);
            item.Name = Path.GetFileNameWithoutExtension(decision.LocalTrack.Path);
            item.DownloadId = downloadId;

            if (decision.LocalTrack.Artist != null)
            {
                item.Artist = decision.LocalTrack.Artist;
            }

            if (decision.LocalTrack.Album != null)
            {
                item.Album = decision.LocalTrack.Album;
            }

            if (decision.LocalTrack.Tracks.Any())
            {
                item.Tracks = decision.LocalTrack.Tracks;
            }

            item.Quality = decision.LocalTrack.Quality;
            item.Language = decision.LocalTrack.Language;
            item.Size = _diskProvider.GetFileSize(decision.LocalTrack.Path);
            item.Rejections = decision.Rejections;

            return item;
        }

        public void Execute(ManualImportCommand message)
        {
            _logger.ProgressTrace("Manually importing {0} files using mode {1}", message.Files.Count, message.ImportMode);

            var imported = new List<ImportResult>();
            var importedTrackedDownload = new List<ManuallyImportedFile>();

            for (int i = 0; i < message.Files.Count; i++)
            {
                _logger.ProgressTrace("Processing file {0} of {1}", i + 1, message.Files.Count);

                var file = message.Files[i];
                var artist = _artistService.GetArtist(file.ArtistId);
                var album = _albumService.GetAlbum(file.AlbumId);
                var tracks = _trackService.GetTracks(file.TrackIds);
                var parsedTrackInfo = Parser.Parser.ParseMusicPath(file.Path) ?? new ParsedTrackInfo();
                var mediaInfo = _videoFileInfoReader.GetMediaInfo(file.Path);
                var existingFile = artist.Path.IsParentPath(file.Path);

                var localTrack = new LocalTrack
                {
                    ExistingFile = false,
                    Tracks = tracks,
                    MediaInfo = mediaInfo,
                    ParsedTrackInfo = parsedTrackInfo,
                    Path = file.Path,
                    Quality = file.Quality,
                    Language = file.Language,
                    Artist = artist,
                    Album = album,
                    Size = 0
                };

                //TODO: Cleanup non-tracked downloads

                var importDecision = new ImportDecision(localTrack);

                if (file.DownloadId.IsNullOrWhiteSpace())
                {
                    imported.AddRange(_importApprovedTracks.Import(new List<ImportDecision> { importDecision }, !existingFile, null, message.ImportMode));
                }

                else
                {
                    var trackedDownload = _trackedDownloadService.Find(file.DownloadId);
                    var importResult = _importApprovedTracks.Import(new List<ImportDecision> { importDecision }, true, trackedDownload.DownloadItem, message.ImportMode).First();

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

                if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath.FullPath))
                {
                    if (_downloadedEpisodesImportService.ShouldDeleteFolder(
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
