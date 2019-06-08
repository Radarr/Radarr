using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Music
{
    public interface IRefreshTrackService
    {
        bool RefreshTrackInfo(Album rg, bool forceUpdateFileTags);
    }

    public class RefreshTrackService : IRefreshTrackService
    {
        private readonly ITrackService _trackService;
        private readonly IAlbumService _albumService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAudioTagService _audioTagService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public RefreshTrackService(ITrackService trackService,
                                   IAlbumService albumService,
                                   IMediaFileService mediaFileService,
                                   IAudioTagService audioTagService,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _trackService = trackService;
            _albumService = albumService;
            _mediaFileService = mediaFileService;
            _audioTagService = audioTagService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public bool RefreshTrackInfo(Album album, bool forceUpdateFileTags)
        {
            _logger.Info("Starting track info refresh for: {0}", album);
            var successCount = 0;
            var failCount = 0;
            bool updated = false;

            foreach (var release in album.AlbumReleases.Value)
            {
                var remoteTracks = release.Tracks.Value.DistinctBy(m => m.ForeignTrackId).ToList();
                var existingTracks = _trackService.GetTracksForRefresh(release.Id, remoteTracks.Select(x => x.ForeignTrackId));

                var updateList = new List<Track>();
                var newList = new List<Track>();
                var upToDateList = new List<Track>();

                foreach (var track in remoteTracks)
                {
                    track.AlbumRelease = release;
                    track.AlbumReleaseId = release.Id;
                    // the artist metadata will have been inserted by RefreshAlbumInfo so the Id will now be populated
                    track.ArtistMetadataId = track.ArtistMetadata.Value.Id;
                    
                    try
                    {
                        var trackToUpdate = existingTracks.SingleOrDefault(e => e.ForeignTrackId == track.ForeignTrackId);
                        if (trackToUpdate != null)
                        {
                            existingTracks.Remove(trackToUpdate);

                            // populate albumrelease for later
                            trackToUpdate.AlbumRelease = release;
                            
                            // copy across the db keys to the remote track and check if we need to update
                            track.Id = trackToUpdate.Id;
                            track.TrackFileId = trackToUpdate.TrackFileId;
                            // make sure title is not null
                            track.Title = track.Title ?? "Unknown";

                            if (!trackToUpdate.Equals(track))
                            {
                                updateList.Add(track);
                            }
                            else
                            {
                                upToDateList.Add(track);
                            }
                        }
                        else
                        {
                            newList.Add(track);
                        }

                        successCount++;
                    }
                    catch (Exception e)
                    {
                        _logger.Fatal(e, "An error has occurred while updating track info for album {0}. {1}", album, track);
                        failCount++;
                    }
                }

                // if any tracks with files are deleted, strip out the MB tags from the metadata
                // so that we stand a chance of matching next time
                _audioTagService.RemoveMusicBrainzTags(existingTracks);

                var tagsToUpdate = updateList;
                if (forceUpdateFileTags)
                {
                    _logger.Debug("Forcing tag update due to Artist/Album/Release updates");
                    tagsToUpdate = updateList.Concat(upToDateList).ToList();
                }
                _audioTagService.SyncTags(tagsToUpdate);
                
                _logger.Debug($"{release}: {upToDateList.Count} tracks up to date; Deleting {existingTracks.Count}, Updating {updateList.Count}, Adding {newList.Count} tracks.");

                _trackService.DeleteMany(existingTracks);
                _trackService.UpdateMany(updateList);
                _trackService.InsertMany(newList);

                updated |= existingTracks.Any() || updateList.Any() || newList.Any();
            }

            if (failCount != 0)
            {
                _logger.Info("Finished track refresh for album: {0}. Successful: {1} - Failed: {2} ",
                    album.Title, successCount, failCount);
            }
            else
            {
                _logger.Info("Finished track refresh for album: {0}.", album);
            }

            return updated;
        }
    }
}

