using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras;
using NzbDrone.Core.Languages;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public interface IImportApprovedTracks
    {
        List<ImportResult> Import(List<ImportDecision<LocalTrack>> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto);
    }

    public class ImportApprovedTracks : IImportApprovedTracks
    {
        private readonly IUpgradeMediaFiles _trackFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAudioTagService _audioTagService;
        private readonly ITrackService _trackService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IExtraService _extraService;
        private readonly IDiskProvider _diskProvider;
        private readonly IReleaseService _releaseService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportApprovedTracks(IUpgradeMediaFiles trackFileUpgrader,
                                    IMediaFileService mediaFileService,
                                    IAudioTagService audioTagService,
                                    ITrackService trackService,
                                    IRecycleBinProvider recycleBinProvider,
                                    IExtraService extraService,
                                    IDiskProvider diskProvider,
                                    IReleaseService releaseService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _trackFileUpgrader = trackFileUpgrader;
            _mediaFileService = mediaFileService;
            _audioTagService = audioTagService;
            _trackService = trackService;
            _recycleBinProvider = recycleBinProvider;
            _extraService = extraService;
            _diskProvider = diskProvider;
            _releaseService = releaseService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision<LocalTrack>> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto)
        {
            var qualifiedImports = decisions.Where(c => c.Approved)
               .GroupBy(c => c.Item.Artist.Id, (i, s) => s
                   .OrderByDescending(c => c.Item.Quality, new QualityModelComparer(s.First().Item.Artist.QualityProfile))
                   .ThenByDescending(c => c.Item.Language, new LanguageComparer(s.First().Item.Artist.LanguageProfile))
                   .ThenByDescending(c => c.Item.Size))
               .SelectMany(c => c)
               .ToList();

            var importResults = new List<ImportResult>();
            var allImportedTrackFiles = new List<TrackFile>();
            var allOldTrackFiles = new List<TrackFile>();

            var albumDecisions = decisions.Where(e => e.Item.Album != null)
                .GroupBy(e => e.Item.Album.Id).ToList();

            foreach (var albumDecision in albumDecisions)
            {
                var album = albumDecision.First().Item.Album;
                var currentRelease = album.AlbumReleases.Value.Single(x => x.Monitored);

                if (albumDecision.Any(x => x.Approved))
                {
                    var newRelease = albumDecision.First(x => x.Approved).Item.Release;

                    if (currentRelease.Id != newRelease.Id)
                    {
                        // if we are importing a new release, delete all old files and don't attempt to upgrade
                        if (newDownload)
                        {
                            var artist = albumDecision.First().Item.Artist;
                            var rootFolder = _diskProvider.GetParentFolder(artist.Path);
                            var previousFiles = _mediaFileService.GetFilesByAlbum(album.Id);

                            foreach (var previousFile in previousFiles)
                            {
                                var trackFilePath = Path.Combine(artist.Path, previousFile.RelativePath);
                                var subfolder = rootFolder.GetRelativePath(_diskProvider.GetParentFolder(trackFilePath));
                                if (_diskProvider.FileExists(trackFilePath))
                                {
                                    _logger.Debug("Removing existing track file: {0}", previousFile);
                                    _recycleBinProvider.DeleteFile(trackFilePath, subfolder);
                                }
                                _mediaFileService.Delete(previousFile, DeleteMediaFileReason.Upgrade);
                            }
                        }

                        // set the correct release to be monitored before importing the new files
                        _logger.Debug("Updating release to {0} [{1} tracks]", newRelease, newRelease.TrackCount);
                        _releaseService.SetMonitored(newRelease);

                        // Publish album edited event.
                        // Deliberatly don't put in the old album since we don't want to trigger an ArtistScan.
                        _eventAggregator.PublishEvent(new AlbumEditedEvent(album, album));
                    }
                }
            }

            var filesToAdd = new List<TrackFile>(qualifiedImports.Count);
            var albumReleasesDict = new Dictionary<int, List<AlbumRelease>>(albumDecisions.Count);
            
            foreach (var importDecision in qualifiedImports.OrderBy(e => e.Item.Tracks.Select(track => track.AbsoluteTrackNumber).MinOrDefault())
                                                           .ThenByDescending(e => e.Item.Size))
            {
                var localTrack = importDecision.Item;
                var oldFiles = new List<TrackFile>();

                try
                {
                    //check if already imported
                    if (importResults.SelectMany(r => r.ImportDecision.Item.Tracks)
                                         .Select(e => e.Id)
                                         .Intersect(localTrack.Tracks.Select(e => e.Id))
                                         .Any())
                    {
                        importResults.Add(new ImportResult(importDecision, "Track has already been imported"));
                        continue;
                    }

                    // cache album releases and set artist to speed up firing the TrackImported events
                    // (otherwise they'll be retrieved from the DB for each track)
                    if (!albumReleasesDict.ContainsKey(localTrack.Album.Id))
                    {
                        albumReleasesDict.Add(localTrack.Album.Id, localTrack.Album.AlbumReleases.Value);
                    }
                    if (!localTrack.Album.AlbumReleases.IsLoaded)
                    {
                        localTrack.Album.AlbumReleases = albumReleasesDict[localTrack.Album.Id];
                    }
                    localTrack.Album.Artist = localTrack.Artist;

                    foreach (var track in localTrack.Tracks)
                    {
                        track.Artist = localTrack.Artist;
                        track.AlbumRelease = localTrack.Release;
                        track.Album = localTrack.Album;
                    }
                    
                    var trackFile = new TrackFile {
                        Path = localTrack.Path.CleanFilePath(),
                        Size = _diskProvider.GetFileSize(localTrack.Path),
                        DateAdded = DateTime.UtcNow,
                        ReleaseGroup = localTrack.ReleaseGroup,
                        Quality = localTrack.Quality,
                        MediaInfo = localTrack.FileTrackInfo.MediaInfo,
                        Language = localTrack.Language,
                        AlbumId = localTrack.Album.Id,
                        Artist = localTrack.Artist,
                        Album = localTrack.Album,
                        Tracks = localTrack.Tracks
                    };

                    bool copyOnly;
                    switch (importMode)
                    {
                        default:
                        case ImportMode.Auto:
                            copyOnly = downloadClientItem != null && !downloadClientItem.CanMoveFiles;
                            break;
                        case ImportMode.Move:
                            copyOnly = false;
                            break;
                        case ImportMode.Copy:
                            copyOnly = true;
                            break;
                    }

                    if (newDownload)
                    {
                        trackFile.SceneName = GetSceneReleaseName(downloadClientItem, localTrack);

                        var moveResult = _trackFileUpgrader.UpgradeTrackFile(trackFile, localTrack, copyOnly);
                        oldFiles = moveResult.OldFiles;
                    }
                    else
                    {
                        trackFile.RelativePath = localTrack.Artist.Path.GetRelativePath(trackFile.Path);

                        // Delete existing files from the DB mapped to this path
                        var previousFiles = _mediaFileService.GetFilesWithRelativePath(localTrack.Artist.Id, trackFile.RelativePath);

                        foreach (var previousFile in previousFiles)
                        {
                            _mediaFileService.Delete(previousFile, DeleteMediaFileReason.ManualOverride);
                        }

                        _audioTagService.WriteTags(trackFile, newDownload);
                    }

                    filesToAdd.Add(trackFile);
                    importResults.Add(new ImportResult(importDecision));

                    if (newDownload)
                    {
                        _extraService.ImportTrack(localTrack, trackFile, copyOnly);
                    }

                    allImportedTrackFiles.Add(trackFile);
                    allOldTrackFiles.AddRange(oldFiles);

                    _eventAggregator.PublishEvent(new TrackImportedEvent(localTrack, trackFile, oldFiles, newDownload, downloadClientItem));
                }
                catch (RootFolderNotFoundException e)
                {
                    _logger.Warn(e, "Couldn't import track " + localTrack);
                    _eventAggregator.PublishEvent(new TrackImportFailedEvent(e, localTrack, newDownload, downloadClientItem));

                    importResults.Add(new ImportResult(importDecision, "Failed to import track, Root folder missing."));
                }
                catch (DestinationAlreadyExistsException e)
                {
                    _logger.Warn(e, "Couldn't import track " + localTrack);
                    importResults.Add(new ImportResult(importDecision, "Failed to import track, Destination already exists."));
                }
                catch (UnauthorizedAccessException e)
                {
                    _logger.Warn(e, "Couldn't import track " + localTrack);
                    importResults.Add(new ImportResult(importDecision, "Failed to import track, Permissions error"));
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Couldn't import track " + localTrack);
                    importResults.Add(new ImportResult(importDecision, "Failed to import track"));
                }
            }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            _mediaFileService.AddMany(filesToAdd);
            _logger.Debug($"Inserted new trackfiles in {watch.ElapsedMilliseconds}ms");
            filesToAdd.ForEach(f => f.Tracks.Value.ForEach(t => t.TrackFileId = f.Id));
            _trackService.SetFileIds(filesToAdd.SelectMany(x => x.Tracks.Value).ToList());
            _logger.Debug($"TrackFileIds updated, total {watch.ElapsedMilliseconds}ms");
            
            var albumImports = importResults.Where(e => e.ImportDecision.Item.Album != null)
                .GroupBy(e => e.ImportDecision.Item.Album.Id).ToList();

            foreach (var albumImport in albumImports)
            {
                var release = albumImport.First().ImportDecision.Item.Release;
                var album = albumImport.First().ImportDecision.Item.Album;
                var artist = albumImport.First().ImportDecision.Item.Artist;

                if (albumImport.Where(e => e.Errors.Count == 0).ToList().Count > 0 && artist != null && album != null)
                {
                    _eventAggregator.PublishEvent(new AlbumImportedEvent(
                        artist,
                        album,
                        release,
                        allImportedTrackFiles.Where(s => s.AlbumId == album.Id).ToList(),
                        allOldTrackFiles.Where(s => s.AlbumId == album.Id).ToList(), newDownload,
                        downloadClientItem));
                }

            }

            //Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.Select(r => r.Reason).ToArray())));

            return importResults;
        }

        private string GetSceneReleaseName(DownloadClientItem downloadClientItem, LocalTrack localTrack)
        {
            if (downloadClientItem != null)
            {
                var title = Parser.Parser.RemoveFileExtension(downloadClientItem.Title);

                var parsedTitle = Parser.Parser.ParseAlbumTitle(title);

                if (parsedTitle != null)
                {
                    return title;
                }
            }

            return null;
        }

    }
}
