using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Common.EnsureThat;

namespace NzbDrone.Core.Music
{
    public interface IRefreshAlbumService
    {
        bool RefreshAlbumInfo(Album album, bool forceUpdateFileTags);
        bool RefreshAlbumInfo(List<Album> albums, bool forceAlbumRefresh, bool forceUpdateFileTags);
    }

    public class RefreshAlbumService : IRefreshAlbumService, IExecute<RefreshAlbumCommand>
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IArtistMetadataRepository _artistMetadataRepository;
        private readonly IReleaseService _releaseService;
        private readonly IProvideAlbumInfo _albumInfo;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly IAudioTagService _audioTagService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICheckIfAlbumShouldBeRefreshed _checkIfAlbumShouldBeRefreshed;
        private readonly Logger _logger;

        public RefreshAlbumService(IAlbumService albumService,
                                   IArtistService artistService,
                                   IArtistMetadataRepository artistMetadataRepository,
                                   IReleaseService releaseService,
                                   IProvideAlbumInfo albumInfo,
                                   IRefreshTrackService refreshTrackService,
                                   IAudioTagService audioTagService,
                                   IEventAggregator eventAggregator,
                                   ICheckIfAlbumShouldBeRefreshed checkIfAlbumShouldBeRefreshed,
                                   Logger logger)
        {
            _albumService = albumService;
            _artistService = artistService;
            _artistMetadataRepository = artistMetadataRepository;
            _releaseService = releaseService;
            _albumInfo = albumInfo;
            _refreshTrackService = refreshTrackService;
            _audioTagService = audioTagService;
            _eventAggregator = eventAggregator;
            _checkIfAlbumShouldBeRefreshed = checkIfAlbumShouldBeRefreshed;
            _logger = logger;
        }

        public bool RefreshAlbumInfo(List<Album> albums, bool forceAlbumRefresh, bool forceUpdateFileTags)
        {
            bool updated = false;
            foreach (var album in albums)
            {
                if (forceAlbumRefresh || _checkIfAlbumShouldBeRefreshed.ShouldRefresh(album))
                {
                    updated |= RefreshAlbumInfo(album, forceUpdateFileTags);
                }
            }
            return updated;
        }

        public bool RefreshAlbumInfo(Album album, bool forceUpdateFileTags)
        {
            _logger.ProgressInfo("Updating Info for {0}", album.Title);
            bool updated = false;

            Tuple<string, Album, List<ArtistMetadata>> tuple;

            try
            {
                tuple = _albumInfo.GetAlbumInfo(album.ForeignAlbumId);
            }
            catch (AlbumNotFoundException)
            {
                _logger.Error($"{album} was not found, it may have been removed from Metadata sources.");
                return updated;
            }

            if (tuple.Item2.AlbumReleases.Value.Count == 0)
            {
                _logger.Debug($"{album} has no valid releases, removing.");
                _albumService.DeleteMany(new List<Album> { album });
                return true;
            }

            var remoteMetadata = tuple.Item3.DistinctBy(x => x.ForeignArtistId).ToList();
            var existingMetadata = _artistMetadataRepository.FindById(remoteMetadata.Select(x => x.ForeignArtistId).ToList());
            var updateMetadataList = new List<ArtistMetadata>();
            var addMetadataList = new List<ArtistMetadata>();
            var upToDateMetadataCount = 0;

            foreach (var meta in remoteMetadata)
            {
                var existing = existingMetadata.SingleOrDefault(x => x.ForeignArtistId == meta.ForeignArtistId);
                if (existing != null)
                {
                    meta.Id = existing.Id;
                    if (!meta.Equals(existing))
                    {
                        updateMetadataList.Add(meta);
                    }
                    else
                    {
                        upToDateMetadataCount++;
                    }
                }
                else
                {
                    addMetadataList.Add(meta);
                }
            }

            _logger.Debug($"{album}: {upToDateMetadataCount} artist metadata up to date; Updating {updateMetadataList.Count}, Adding {addMetadataList.Count} artist metadata entries.");

            _artistMetadataRepository.UpdateMany(updateMetadataList);
            _artistMetadataRepository.InsertMany(addMetadataList);

            forceUpdateFileTags |= updateMetadataList.Any();
            updated |= updateMetadataList.Any() || addMetadataList.Any();

            var albumInfo = tuple.Item2;

            if (album.ForeignAlbumId != albumInfo.ForeignAlbumId)
            {
                _logger.Warn(
                    "Album '{0}' (Album {1}) was replaced with '{2}' (LidarrAPI {3}), because the original was a duplicate.",
                    album.Title, album.ForeignAlbumId, albumInfo.Title, albumInfo.ForeignAlbumId);
                album.ForeignAlbumId = albumInfo.ForeignAlbumId;
            }

            // the only thing written to tags from the album object is the title
            forceUpdateFileTags |= album.Title != (albumInfo.Title ?? "Unknown");
            updated |= forceUpdateFileTags;

            album.OldForeignAlbumIds = albumInfo.OldForeignAlbumIds;
            album.LastInfoSync = DateTime.UtcNow;
            album.CleanTitle = albumInfo.CleanTitle;
            album.Title = albumInfo.Title ?? "Unknown";
            album.Overview = albumInfo.Overview.IsNullOrWhiteSpace() ? album.Overview : albumInfo.Overview;
            album.Disambiguation = albumInfo.Disambiguation;
            album.AlbumType = albumInfo.AlbumType;
            album.SecondaryTypes = albumInfo.SecondaryTypes;
            album.Genres = albumInfo.Genres;
            album.Images = albumInfo.Images.Any() ? albumInfo.Images : album.Images;
            album.Links = albumInfo.Links;
            album.ReleaseDate = albumInfo.ReleaseDate;
            album.Ratings = albumInfo.Ratings;
            album.AlbumReleases = new List<AlbumRelease>();

            var remoteReleases = albumInfo.AlbumReleases.Value.DistinctBy(m => m.ForeignReleaseId).ToList();
            var existingReleases = _releaseService.GetReleasesForRefresh(album.Id, remoteReleases.Select(x => x.ForeignReleaseId));
            // Keep track of which existing release we want to end up monitored
            var existingToMonitor = existingReleases.Where(x => x.Monitored).OrderByDescending(x => x.TrackCount).FirstOrDefault();

            var newReleaseList = new List<AlbumRelease>();
            var updateReleaseList = new List<AlbumRelease>();
            var upToDateReleaseList = new List<AlbumRelease>();

            foreach (var release in remoteReleases)
            {
                release.AlbumId = album.Id;
                release.Album = album;

                // force to unmonitored, then fix monitored one later
                // once we have made sure that it's unique.  This make sure
                // that we unmonitor anything in database that shouldn't be monitored.
                release.Monitored = false;

                var releaseToRefresh = existingReleases.SingleOrDefault(r => r.ForeignReleaseId == release.ForeignReleaseId);

                if (releaseToRefresh != null)
                {
                    existingReleases.Remove(releaseToRefresh);

                    // copy across the db keys and check for equality
                    release.Id = releaseToRefresh.Id;
                    release.AlbumId = releaseToRefresh.AlbumId;
                    
                    if (!releaseToRefresh.Equals(release))
                    {
                        updateReleaseList.Add(release);
                    }
                    else
                    {
                        upToDateReleaseList.Add(release);
                    }
                }
                else
                {
                    newReleaseList.Add(release);
                }
                
                album.AlbumReleases.Value.Add(release);
            }
            
            var refreshedToMonitor = remoteReleases.SingleOrDefault(x => x.ForeignReleaseId == existingToMonitor?.ForeignReleaseId) ??
                remoteReleases.OrderByDescending(x => x.TrackCount).First();
            refreshedToMonitor.Monitored = true;
            
            if (upToDateReleaseList.Contains(refreshedToMonitor))
            {
                // we weren't going to update, but have changed monitored so now need to
                upToDateReleaseList.Remove(refreshedToMonitor);
                updateReleaseList.Add(refreshedToMonitor);
            }
            else if (updateReleaseList.Contains(refreshedToMonitor) && refreshedToMonitor.Equals(existingToMonitor))
            {
                // we were going to update because Monitored was incorrect but now it matches
                // and so no need to update
                updateReleaseList.Remove(refreshedToMonitor);
                upToDateReleaseList.Add(refreshedToMonitor);
            }
            
            Ensure.That(album.AlbumReleases.Value.Count(x => x.Monitored) == 1).IsTrue();
            
            _logger.Debug($"{album} {upToDateReleaseList.Count} releases up to date; Deleting {existingReleases.Count}, Updating {updateReleaseList.Count}, Adding {newReleaseList.Count} releases.");

            // before deleting anything, remove musicbrainz ids for things we are deleting
            _audioTagService.RemoveMusicBrainzTags(existingReleases);

            _releaseService.DeleteMany(existingReleases);
            _releaseService.UpdateMany(updateReleaseList);
            _releaseService.InsertMany(newReleaseList);

            // if we have updated a monitored release, refresh all file tags
            forceUpdateFileTags |= updateReleaseList.Any(x => x.Monitored);
            updated |= existingReleases.Any() || updateReleaseList.Any() || newReleaseList.Any();

            updated |= _refreshTrackService.RefreshTrackInfo(album, forceUpdateFileTags);
            _albumService.UpdateMany(new List<Album>{album});

            _logger.Debug("Finished album refresh for {0}", album.Title);

            return updated;
        }

        public void Execute(RefreshAlbumCommand message)
        {
            if (message.AlbumId.HasValue)
            {
                var album = _albumService.GetAlbum(message.AlbumId.Value);
                var artist = _artistService.GetArtistByMetadataId(album.ArtistMetadataId);
                var updated = RefreshAlbumInfo(album, false);
                if (updated)
                {
                    _eventAggregator.PublishEvent(new ArtistUpdatedEvent(artist));
                }
            }
        }
    }
}

