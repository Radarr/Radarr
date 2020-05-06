using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles.TrackImport
{
    public interface IImportApprovedTracks
    {
        List<ImportResult> Import(List<ImportDecision<LocalTrack>> decisions, bool replaceExisting, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto);
    }

    public class ImportApprovedTracks : IImportApprovedTracks
    {
        private readonly IUpgradeMediaFiles _trackFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAudioTagService _audioTagService;
        private readonly IArtistService _artistService;
        private readonly IAddArtistService _addArtistService;
        private readonly IAlbumService _albumService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IExtraService _extraService;
        private readonly IDiskProvider _diskProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public ImportApprovedTracks(IUpgradeMediaFiles trackFileUpgrader,
                                    IMediaFileService mediaFileService,
                                    IAudioTagService audioTagService,
                                    IArtistService artistService,
                                    IAddArtistService addArtistService,
                                    IAlbumService albumService,
                                    IRootFolderService rootFolderService,
                                    IRecycleBinProvider recycleBinProvider,
                                    IExtraService extraService,
                                    IDiskProvider diskProvider,
                                    IEventAggregator eventAggregator,
                                    IManageCommandQueue commandQueueManager,
                                    Logger logger)
        {
            _trackFileUpgrader = trackFileUpgrader;
            _mediaFileService = mediaFileService;
            _audioTagService = audioTagService;
            _artistService = artistService;
            _addArtistService = addArtistService;
            _albumService = albumService;
            _rootFolderService = rootFolderService;
            _recycleBinProvider = recycleBinProvider;
            _extraService = extraService;
            _diskProvider = diskProvider;
            _eventAggregator = eventAggregator;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision<LocalTrack>> decisions, bool replaceExisting, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto)
        {
            var importResults = new List<ImportResult>();
            var allImportedTrackFiles = new List<BookFile>();
            var allOldTrackFiles = new List<BookFile>();
            var addedArtists = new List<Author>();

            var albumDecisions = decisions.Where(e => e.Item.Album != null && e.Approved)
                .GroupBy(e => e.Item.Album.ForeignBookId).ToList();

            int iDecision = 1;
            foreach (var albumDecision in albumDecisions)
            {
                _logger.ProgressInfo($"Importing book {iDecision++}/{albumDecisions.Count} {albumDecision.First().Item.Album}");

                var decisionList = albumDecision.ToList();

                var artist = EnsureArtistAdded(decisionList, addedArtists);

                if (artist == null)
                {
                    // failed to add the artist, carry on with next album
                    continue;
                }

                var album = EnsureAlbumAdded(decisionList);

                if (album == null)
                {
                    // failed to add the album, carry on with next one
                    continue;
                }

                if (replaceExisting)
                {
                    RemoveExistingTrackFiles(artist, album);
                }

                // Publish album edited event.
                // Deliberatly don't put in the old album since we don't want to trigger an ArtistScan.
                _eventAggregator.PublishEvent(new AlbumEditedEvent(album, album));
            }

            var qualifiedImports = decisions.Where(c => c.Approved)
                .GroupBy(c => c.Item.Artist.Id, (i, s) => s
                         .OrderByDescending(c => c.Item.Quality, new QualityModelComparer(s.First().Item.Artist.QualityProfile))
                         .ThenByDescending(c => c.Item.Size))
                .SelectMany(c => c)
                .ToList();

            _logger.ProgressInfo($"Importing {qualifiedImports.Count} files");
            _logger.Debug($"Importing {qualifiedImports.Count} files. replaceExisting: {replaceExisting}");

            var filesToAdd = new List<BookFile>(qualifiedImports.Count);
            var trackImportedEvents = new List<TrackImportedEvent>(qualifiedImports.Count);

            foreach (var importDecision in qualifiedImports.OrderByDescending(e => e.Item.Size))
            {
                var localTrack = importDecision.Item;
                var oldFiles = new List<BookFile>();

                try
                {
                    //check if already imported
                    if (importResults.Select(r => r.ImportDecision.Item.Album.Id).Contains(localTrack.Album.Id))
                    {
                        importResults.Add(new ImportResult(importDecision, "Book has already been imported"));
                        continue;
                    }

                    localTrack.Album.Author = localTrack.Artist;

                    var trackFile = new BookFile
                    {
                        Path = localTrack.Path.CleanFilePath(),
                        Size = localTrack.Size,
                        Modified = localTrack.Modified,
                        DateAdded = DateTime.UtcNow,
                        ReleaseGroup = localTrack.ReleaseGroup,
                        Quality = localTrack.Quality,
                        MediaInfo = localTrack.FileTrackInfo.MediaInfo,
                        BookId = localTrack.Album.Id,
                        Artist = localTrack.Artist,
                        Album = localTrack.Album
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

                    if (!localTrack.ExistingFile)
                    {
                        trackFile.SceneName = GetSceneReleaseName(downloadClientItem);

                        var moveResult = _trackFileUpgrader.UpgradeTrackFile(trackFile, localTrack, copyOnly);
                        oldFiles = moveResult.OldFiles;
                    }
                    else
                    {
                        // Delete existing files from the DB mapped to this path
                        var previousFile = _mediaFileService.GetFileWithPath(trackFile.Path);

                        if (previousFile != null)
                        {
                            _mediaFileService.Delete(previousFile, DeleteMediaFileReason.ManualOverride);
                        }

                        var rootFolder = _rootFolderService.GetBestRootFolder(localTrack.Path);
                        if (rootFolder.IsCalibreLibrary)
                        {
                            trackFile.CalibreId = trackFile.Path.ParseCalibreId();
                        }

                        _audioTagService.WriteTags(trackFile, false);
                    }

                    filesToAdd.Add(trackFile);
                    importResults.Add(new ImportResult(importDecision));

                    if (!localTrack.ExistingFile)
                    {
                        _extraService.ImportTrack(localTrack, trackFile, copyOnly);
                    }

                    allImportedTrackFiles.Add(trackFile);
                    allOldTrackFiles.AddRange(oldFiles);

                    // create all the import events here, but we can't publish until the trackfiles have been
                    // inserted and ids created
                    trackImportedEvents.Add(new TrackImportedEvent(localTrack, trackFile, oldFiles, !localTrack.ExistingFile, downloadClientItem));
                }
                catch (RootFolderNotFoundException e)
                {
                    _logger.Warn(e, "Couldn't import track " + localTrack);
                    _eventAggregator.PublishEvent(new TrackImportFailedEvent(e, localTrack, !localTrack.ExistingFile, downloadClientItem));

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
                    _eventAggregator.PublishEvent(new TrackImportFailedEvent(e, localTrack, !localTrack.ExistingFile, downloadClientItem));

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

            // now that trackfiles have been inserted and ids generated, publish the import events
            foreach (var trackImportedEvent in trackImportedEvents)
            {
                _eventAggregator.PublishEvent(trackImportedEvent);
            }

            var albumImports = importResults.Where(e => e.ImportDecision.Item.Album != null)
                .GroupBy(e => e.ImportDecision.Item.Album.Id).ToList();

            foreach (var albumImport in albumImports)
            {
                var album = albumImport.First().ImportDecision.Item.Album;
                var artist = albumImport.First().ImportDecision.Item.Artist;

                if (albumImport.Where(e => e.Errors.Count == 0).ToList().Count > 0 && artist != null && album != null)
                {
                    _eventAggregator.PublishEvent(new AlbumImportedEvent(
                        artist,
                        album,
                        allImportedTrackFiles.Where(s => s.BookId == album.Id).ToList(),
                        allOldTrackFiles.Where(s => s.BookId == album.Id).ToList(),
                        replaceExisting,
                        downloadClientItem));
                }
            }

            //Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.Select(r => r.Reason).ToArray())));

            // Refresh any artists we added
            if (addedArtists.Any())
            {
                _commandQueueManager.Push(new BulkRefreshArtistCommand(addedArtists.Select(x => x.Id).ToList(), true));
            }

            return importResults;
        }

        private Author EnsureArtistAdded(List<ImportDecision<LocalTrack>> decisions, List<Author> addedArtists)
        {
            var artist = decisions.First().Item.Artist;

            if (artist.Id == 0)
            {
                var dbArtist = _artistService.FindById(artist.ForeignAuthorId);

                if (dbArtist == null)
                {
                    _logger.Debug($"Adding remote artist {artist}");
                    var path = decisions.First().Item.Path;
                    var rootFolder = _rootFolderService.GetBestRootFolder(path);

                    artist.RootFolderPath = rootFolder.Path;
                    artist.MetadataProfileId = rootFolder.DefaultMetadataProfileId;
                    artist.QualityProfileId = rootFolder.DefaultQualityProfileId;
                    artist.Monitored = rootFolder.DefaultMonitorOption != MonitorTypes.None;
                    artist.Tags = rootFolder.DefaultTags;
                    artist.AddOptions = new AddArtistOptions
                    {
                        SearchForMissingAlbums = false,
                        Monitored = artist.Monitored,
                        Monitor = rootFolder.DefaultMonitorOption
                    };

                    if (rootFolder.IsCalibreLibrary)
                    {
                        // calibre has artist / book / files
                        artist.Path = path.GetParentPath().GetParentPath();
                    }

                    try
                    {
                        dbArtist = _addArtistService.AddArtist(artist, false);
                        addedArtists.Add(dbArtist);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Failed to add artist {0}", artist);
                        foreach (var decision in decisions)
                        {
                            decision.Reject(new Rejection("Failed to add missing artist", RejectionType.Temporary));
                        }

                        return null;
                    }
                }

                // Put in the newly loaded artist
                foreach (var decision in decisions)
                {
                    decision.Item.Artist = dbArtist;
                    decision.Item.Album.Author = dbArtist;
                    decision.Item.Album.AuthorMetadataId = dbArtist.AuthorMetadataId;
                }

                artist = dbArtist;
            }

            return artist;
        }

        private Book EnsureAlbumAdded(List<ImportDecision<LocalTrack>> decisions)
        {
            var album = decisions.First().Item.Album;

            if (album.Id == 0)
            {
                var dbAlbum = _albumService.FindById(album.ForeignBookId);

                if (dbAlbum == null)
                {
                    _logger.Debug($"Adding remote album {album}");
                    try
                    {
                        album.Added = DateTime.UtcNow;
                        _albumService.InsertMany(new List<Book> { album });
                        dbAlbum = _albumService.FindById(album.ForeignBookId);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Failed to add album {0}", album);
                        RejectAlbum(decisions);

                        return null;
                    }
                }

                // Populate the new DB album
                foreach (var decision in decisions)
                {
                    decision.Item.Album = dbAlbum;
                }
            }

            return album;
        }

        private void RejectAlbum(List<ImportDecision<LocalTrack>> decisions)
        {
            foreach (var decision in decisions)
            {
                decision.Reject(new Rejection("Failed to add missing album", RejectionType.Temporary));
            }
        }

        private void RemoveExistingTrackFiles(Author artist, Book album)
        {
            var rootFolder = _diskProvider.GetParentFolder(artist.Path);
            var previousFiles = _mediaFileService.GetFilesByAlbum(album.Id);

            _logger.Debug($"Deleting {previousFiles.Count} existing files for {album}");

            foreach (var previousFile in previousFiles)
            {
                var subfolder = rootFolder.GetRelativePath(_diskProvider.GetParentFolder(previousFile.Path));
                if (_diskProvider.FileExists(previousFile.Path))
                {
                    _logger.Debug("Removing existing track file: {0}", previousFile);
                    _recycleBinProvider.DeleteFile(previousFile.Path, subfolder);
                }

                _mediaFileService.Delete(previousFile, DeleteMediaFileReason.Upgrade);
            }
        }

        private string GetSceneReleaseName(DownloadClientItem downloadClientItem)
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
