using System;
using System.Collections.Generic;
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
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public interface IImportApprovedTracks
    {
        List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto);
    }

    public class ImportApprovedTracks : IImportApprovedTracks
    {
        private readonly IUpgradeMediaFiles _trackFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IExtraService _extraService;
        private readonly IDiskProvider _diskProvider;
        private readonly IReleaseService _releaseService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ImportApprovedTracks(IUpgradeMediaFiles trackFileUpgrader,
                                    IMediaFileService mediaFileService,
                                    IExtraService extraService,
                                    IDiskProvider diskProvider,
                                    IReleaseService releaseService,
                                    IEventAggregator eventAggregator,
                                    Logger logger)
        {
            _trackFileUpgrader = trackFileUpgrader;
            _mediaFileService = mediaFileService;
            _extraService = extraService;
            _diskProvider = diskProvider;
            _releaseService = releaseService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision> decisions, bool newDownload, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto)
        {
            var qualifiedImports = decisions.Where(c => c.Approved)
               .GroupBy(c => c.LocalTrack.Artist.Id, (i, s) => s
                   .OrderByDescending(c => c.LocalTrack.Quality, new QualityModelComparer(s.First().LocalTrack.Artist.Profile))
                   .ThenByDescending(c => c.LocalTrack.Language, new LanguageComparer(s.First().LocalTrack.Artist.LanguageProfile))
                   .ThenByDescending(c => c.LocalTrack.Size))
               .SelectMany(c => c)
               .ToList();

            var importResults = new List<ImportResult>();
            var allImportedTrackFiles = new List<TrackFile>();
            var allOldTrackFiles = new List<TrackFile>();

            var albumDecisions = decisions.Where(e => e.LocalTrack.Album != null)
                .GroupBy(e => e.LocalTrack.Album.Id).ToList();

            foreach (var albumDecision in albumDecisions)
            {
                // set the correct release to be monitored after doing the import
                var album = albumDecision.First().LocalTrack.Album;
                var release = albumDecision.First().LocalTrack.Release;
                _logger.Debug("Updating release to {0} [{1} tracks]", release, release.TrackCount);
                _releaseService.SetMonitored(release);

                // Publish album edited event.
                // Deliberatly don't put in the old album since we don't want to trigger an ArtistScan.
                _eventAggregator.PublishEvent(new AlbumEditedEvent(album, album));
            }

            foreach (var importDecision in qualifiedImports.OrderBy(e => e.LocalTrack.Tracks.Select(track => track.AbsoluteTrackNumber).MinOrDefault())
                                                           .ThenByDescending(e => e.LocalTrack.Size))
            {
                var localTrack = importDecision.LocalTrack;
                var oldFiles = new List<TrackFile>();

                try
                {
                    //check if already imported
                    if (importResults.SelectMany(r => r.ImportDecision.LocalTrack.Tracks)
                                         .Select(e => e.Id)
                                         .Intersect(localTrack.Tracks.Select(e => e.Id))
                                         .Any())
                    {
                        importResults.Add(new ImportResult(importDecision, "Track has already been imported"));
                        continue;
                    }

                    
                    var trackFile = new TrackFile {
                        Path = localTrack.Path.CleanFilePath(),
                        Size = _diskProvider.GetFileSize(localTrack.Path),
                        DateAdded = DateTime.UtcNow,
                        ReleaseGroup = localTrack.ParsedTrackInfo.ReleaseGroup,
                        Quality = localTrack.Quality,
                        MediaInfo = localTrack.MediaInfo,
                        Language = localTrack.Language,
                        AlbumId = localTrack.Album.Id,
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
                        //trackFile.SceneName = GetSceneName(downloadClientItem, localTrack);

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

                    }

                    _mediaFileService.Add(trackFile);
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

            var albumImports = importResults.Where(e => e.ImportDecision.LocalTrack.Album != null)
                .GroupBy(e => e.ImportDecision.LocalTrack.Album.Id).ToList();

            foreach (var albumImport in albumImports)
            {
                var release = albumImport.First().ImportDecision.LocalTrack.Release;
                var album = albumImport.First().ImportDecision.LocalTrack.Album;
                var artist = albumImport.First().ImportDecision.LocalTrack.Artist;

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
    }
}
