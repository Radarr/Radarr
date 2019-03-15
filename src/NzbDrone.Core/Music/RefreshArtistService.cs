using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NzbDrone.Core.ImportLists.Exclusions;

namespace NzbDrone.Core.Music
{
    public class RefreshArtistService : IExecute<RefreshArtistCommand>
    {
        private readonly IProvideArtistInfo _artistInfo;
        private readonly IArtistService _artistService;
        private readonly IAddAlbumService _addAlbumService;
        private readonly IAlbumService _albumService;
        private readonly IRefreshAlbumService _refreshAlbumService;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly IAudioTagService _audioTagService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDiskScanService _diskScanService;
        private readonly ICheckIfArtistShouldBeRefreshed _checkIfArtistShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly Logger _logger;

        public RefreshArtistService(IProvideArtistInfo artistInfo,
                                    IArtistService artistService,
                                    IAddAlbumService addAlbumService,
                                    IAlbumService albumService,
                                    IRefreshAlbumService refreshAlbumService,
                                    IRefreshTrackService refreshTrackService,
                                    IAudioTagService audioTagService,
                                    IEventAggregator eventAggregator,
                                    IDiskScanService diskScanService,
                                    ICheckIfArtistShouldBeRefreshed checkIfArtistShouldBeRefreshed,
                                    IConfigService configService,
                                    IImportListExclusionService importListExclusionService,
                                    Logger logger)
        {
            _artistInfo = artistInfo;
            _artistService = artistService;
            _addAlbumService = addAlbumService;
            _albumService = albumService;
            _refreshAlbumService = refreshAlbumService;
            _refreshTrackService = refreshTrackService;
            _audioTagService = audioTagService;
            _eventAggregator = eventAggregator;
            _diskScanService = diskScanService;
            _checkIfArtistShouldBeRefreshed = checkIfArtistShouldBeRefreshed;
            _configService = configService;
            _importListExclusionService = importListExclusionService;
            _logger = logger;
        }

        private void RefreshArtistInfo(Artist artist, bool forceAlbumRefresh)
        {
            _logger.ProgressInfo("Updating Info for {0}", artist.Name);

            Artist artistInfo;

            try
            {
                artistInfo = _artistInfo.GetArtistInfo(artist.Metadata.Value.ForeignArtistId, artist.MetadataProfileId);
            }
            catch (ArtistNotFoundException)
            {
                _logger.Error($"Artist {artist} was not found, it may have been removed from Metadata sources.");
                return;
            }

            var forceUpdateFileTags = artist.Name != artistInfo.Name;

            if (artist.Metadata.Value.ForeignArtistId != artistInfo.Metadata.Value.ForeignArtistId)
            {
                _logger.Warn($"Artist {artist} was replaced with {artistInfo} because the original was a duplicate.");

                // Update list exclusion if one exists
                var importExclusion = _importListExclusionService.FindByForeignId(artist.Metadata.Value.ForeignArtistId);

                if (importExclusion != null)
                {
                    importExclusion.ForeignId = artistInfo.Metadata.Value.ForeignArtistId;
                    _importListExclusionService.Update(importExclusion);
                }

                artist.Metadata.Value.ForeignArtistId = artistInfo.Metadata.Value.ForeignArtistId;
                forceUpdateFileTags = true;
            }

            artist.Metadata.Value.ApplyChanges(artistInfo.Metadata.Value);
            artist.CleanName = artistInfo.CleanName;
            artist.SortName = artistInfo.SortName;
            artist.LastInfoSync = DateTime.UtcNow;

            try
            {
                artist.Path = new DirectoryInfo(artist.Path).FullName;
                artist.Path = artist.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Couldn't update artist path for " + artist.Path);
            }

            var remoteAlbums = artistInfo.Albums.Value.DistinctBy(m => m.ForeignAlbumId).ToList();

            // Get list of DB current db albums for artist
            var existingAlbums = _albumService.GetAlbumsForRefresh(artist.ArtistMetadataId, remoteAlbums.Select(x => x.ForeignAlbumId));
            var newAlbumsList = new List<Album>();
            var updateAlbumsList = new List<Album>();

            // Cycle thru albums
            foreach (var album in remoteAlbums)
            {
                // Check for album in existing albums, if not set properties and add to new list
                var albumToRefresh = existingAlbums.SingleOrDefault(s => s.ForeignAlbumId == album.ForeignAlbumId);

                if (albumToRefresh != null)
                {
                    albumToRefresh.Artist = artist;
                    existingAlbums.Remove(albumToRefresh);
                    updateAlbumsList.Add(albumToRefresh);
                }
                else
                {
                    album.Artist = artist;
                    newAlbumsList.Add(album);
                }
            }

            _artistService.UpdateArtist(artist);
            
            _logger.Debug("{0} Deleting {1}, Updating {2}, Adding {3} albums",
                          artist, existingAlbums.Count, updateAlbumsList.Count, newAlbumsList.Count);

            // before deleting anything, remove musicbrainz ids for things we are deleting
            _audioTagService.RemoveMusicBrainzTags(existingAlbums);

            // Delete old albums first - this avoids errors if albums have been merged and we'll
            // end up trying to duplicate an existing release under a new album
            _albumService.DeleteMany(existingAlbums);
            
            // Update new albums with artist info and correct monitored status
            newAlbumsList = UpdateAlbums(artist, newAlbumsList);
            _addAlbumService.AddAlbums(newAlbumsList);

            _refreshAlbumService.RefreshAlbumInfo(updateAlbumsList, forceAlbumRefresh, forceUpdateFileTags);

            _eventAggregator.PublishEvent(new AlbumInfoRefreshedEvent(artist, newAlbumsList, updateAlbumsList));

            _logger.Debug("Finished artist refresh for {0}", artist.Name);
            _eventAggregator.PublishEvent(new ArtistUpdatedEvent(artist));
        }

        private List<Album> UpdateAlbums(Artist artist, List<Album> albumsToUpdate)
        {
            foreach (var album in albumsToUpdate)
            {
                album.ProfileId = artist.QualityProfileId;
                album.Monitored = artist.Monitored;
            }

            return albumsToUpdate;
        }

        private void RescanArtist(Artist artist, bool isNew, CommandTrigger trigger)
        {
            var rescanAfterRefresh = _configService.RescanAfterRefresh;
            var shouldRescan = true;

            if (isNew)
            {
                _logger.Trace("Forcing refresh of {0}. Reason: New artist", artist);
                shouldRescan = true;
            }

            else if (rescanAfterRefresh == RescanAfterRefreshType.Never)
            {
                _logger.Trace("Skipping refresh of {0}. Reason: never recan after refresh", artist);
                shouldRescan = false;
            }

            else if (rescanAfterRefresh == RescanAfterRefreshType.AfterManual && trigger != CommandTrigger.Manual)
            {
                _logger.Trace("Skipping refresh of {0}. Reason: not after automatic scans", artist);
                shouldRescan = false;
            }

            if (!shouldRescan)
            {
                return;
            }

            try
            {
                _diskScanService.Scan(artist);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Couldn't rescan artist {0}", artist);
            }
        }

        public void Execute(RefreshArtistCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewArtist;
            _eventAggregator.PublishEvent(new ArtistRefreshStartingEvent(trigger == CommandTrigger.Manual));

            if (message.ArtistId.HasValue)
            {
                var artist = _artistService.GetArtist(message.ArtistId.Value);

                try
                {
                    RefreshArtistInfo(artist, true);
                    RescanArtist(artist, isNew, trigger);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't refresh info for {0}", artist);
                    RescanArtist(artist, isNew, trigger);
                    throw;
                }
            }
            else
            {
                var allArtists = _artistService.GetAllArtists().OrderBy(c => c.Name).ToList();

                foreach (var artist in allArtists)
                {
                    var manualTrigger = message.Trigger == CommandTrigger.Manual;

                    if (manualTrigger || _checkIfArtistShouldBeRefreshed.ShouldRefresh(artist))
                    {
                        try
                        {
                            RefreshArtistInfo(artist, manualTrigger);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", artist);
                        }

                        RescanArtist(artist, false, trigger);
                    }

                    else
                    {
                        _logger.Info("Skipping refresh of artist: {0}", artist.Name);
                        RescanArtist(artist, false, trigger);
                    }
                }
            }
        }
    }
}
