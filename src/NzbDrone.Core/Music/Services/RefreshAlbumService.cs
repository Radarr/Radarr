using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Music
{
    public interface IRefreshAlbumService
    {
        bool RefreshAlbumInfo(Album album, List<Album> remoteAlbums, bool forceUpdateFileTags);
        bool RefreshAlbumInfo(List<Album> albums, List<Album> remoteAlbums, bool forceAlbumRefresh, bool forceUpdateFileTags, DateTime? lastUpdate);
    }

    public class RefreshAlbumService : RefreshEntityServiceBase<Album, AlbumRelease>, IRefreshAlbumService, IExecute<RefreshAlbumCommand>
    {
        private readonly IAlbumService _albumService;
        private readonly IArtistService _artistService;
        private readonly IAddArtistService _addArtistService;
        private readonly IReleaseService _releaseService;
        private readonly IProvideAlbumInfo _albumInfo;
        private readonly IRefreshAlbumReleaseService _refreshAlbumReleaseService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICheckIfAlbumShouldBeRefreshed _checkIfAlbumShouldBeRefreshed;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly Logger _logger;

        public RefreshAlbumService(IAlbumService albumService,
                                   IArtistService artistService,
                                   IAddArtistService addArtistService,
                                   IArtistMetadataService artistMetadataService,
                                   IReleaseService releaseService,
                                   IProvideAlbumInfo albumInfo,
                                   IRefreshAlbumReleaseService refreshAlbumReleaseService,
                                   IMediaFileService mediaFileService,
                                   IHistoryService historyService,
                                   IEventAggregator eventAggregator,
                                   ICheckIfAlbumShouldBeRefreshed checkIfAlbumShouldBeRefreshed,
                                   IMapCoversToLocal mediaCoverService,
                                   Logger logger)
        : base(logger, artistMetadataService)
        {
            _albumService = albumService;
            _artistService = artistService;
            _addArtistService = addArtistService;
            _releaseService = releaseService;
            _albumInfo = albumInfo;
            _refreshAlbumReleaseService = refreshAlbumReleaseService;
            _mediaFileService = mediaFileService;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _checkIfAlbumShouldBeRefreshed = checkIfAlbumShouldBeRefreshed;
            _mediaCoverService = mediaCoverService;
            _logger = logger;
        }

        protected override RemoteData GetRemoteData(Album local, List<Album> remote)
        {
            var result = new RemoteData();

            // remove not in remote list and ShouldDelete is true
            if (remote != null &&
                !remote.Any(x => x.ForeignAlbumId == local.ForeignAlbumId || x.OldForeignAlbumIds.Contains(local.ForeignAlbumId)) &&
                ShouldDelete(local))
            {
                return result;
            }

            Tuple<string, Album, List<ArtistMetadata>> tuple = null;
            try
            {
                tuple = _albumInfo.GetAlbumInfo(local.ForeignAlbumId);
            }
            catch (AlbumNotFoundException)
            {
                return result;
            }

            if (tuple.Item2.AlbumReleases.Value.Count == 0)
            {
                _logger.Debug($"{local} has no valid releases, removing.");
                return result;
            }

            result.Entity = tuple.Item2;
            result.Entity.Id = local.Id;
            result.Metadata = tuple.Item3;
            return result;
        }

        protected override void EnsureNewParent(Album local, Album remote)
        {
            // Make sure the appropriate artist exists (it could be that an album changes parent)
            // The artistMetadata entry will be in the db but make sure a corresponding artist is too
            // so that the album doesn't just disappear.

            // TODO filter by metadata id before hitting database
            _logger.Trace($"Ensuring parent artist exists [{remote.ArtistMetadata.Value.ForeignArtistId}]");

            var newArtist = _artistService.FindById(remote.ArtistMetadata.Value.ForeignArtistId);

            if (newArtist == null)
            {
                var oldArtist = local.Artist.Value;
                var addArtist = new Artist
                {
                    Metadata = remote.ArtistMetadata.Value,
                    MetadataProfileId = oldArtist.MetadataProfileId,
                    QualityProfileId = oldArtist.QualityProfileId,
                    RootFolderPath = oldArtist.RootFolderPath,
                    Monitored = oldArtist.Monitored,
                    AlbumFolder = oldArtist.AlbumFolder,
                    Tags = oldArtist.Tags
                };
                _logger.Debug($"Adding missing parent artist {addArtist}");
                _addArtistService.AddArtist(addArtist);
            }
        }

        protected override bool ShouldDelete(Album local)
        {
            // not manually added and has no files
            return local.AddOptions.AddType != AlbumAddType.Manual &&
                !_mediaFileService.GetFilesByAlbum(local.Id).Any();
        }

        protected override void LogProgress(Album local)
        {
            _logger.ProgressInfo("Updating Info for {0}", local.Title);
        }

        protected override bool IsMerge(Album local, Album remote)
        {
            return local.ForeignAlbumId != remote.ForeignAlbumId;
        }

        protected override UpdateResult UpdateEntity(Album local, Album remote)
        {
            UpdateResult result;

            remote.UseDbFieldsFrom(local);

            if (local.Title != (remote.Title ?? "Unknown") ||
                local.ForeignAlbumId != remote.ForeignAlbumId ||
                local.ArtistMetadata.Value.ForeignArtistId != remote.ArtistMetadata.Value.ForeignArtistId)
            {
                result = UpdateResult.UpdateTags;
            }
            else if (!local.Equals(remote))
            {
                result = UpdateResult.Standard;
            }
            else
            {
                result = UpdateResult.None;
            }

            // Force update and fetch covers if images have changed so that we can write them into tags
            if (remote.Images.Any() && !local.Images.SequenceEqual(remote.Images))
            {
                _mediaCoverService.EnsureAlbumCovers(remote);
                result = UpdateResult.UpdateTags;
            }

            local.UseMetadataFrom(remote);

            local.ArtistMetadataId = remote.ArtistMetadata.Value.Id;
            local.LastInfoSync = DateTime.UtcNow;
            local.AlbumReleases = new List<AlbumRelease>();

            return result;
        }

        protected override UpdateResult MergeEntity(Album local, Album target, Album remote)
        {
            _logger.Warn($"Album {local} was merged with {remote} because the original was a duplicate.");

            // move releases over to the new album and delete
            var localReleases = _releaseService.GetReleasesByAlbum(local.Id);
            var allReleases = localReleases.Concat(_releaseService.GetReleasesByAlbum(target.Id)).ToList();
            _logger.Trace($"Moving {localReleases.Count} releases from {local} to {remote}");

            // Update album ID and unmonitor all releases from the old album
            allReleases.ForEach(x => x.AlbumId = target.Id);
            MonitorSingleRelease(allReleases);
            _releaseService.UpdateMany(allReleases);

            // Update album ids for trackfiles
            var files = _mediaFileService.GetFilesByAlbum(local.Id);
            files.ForEach(x => x.AlbumId = target.Id);
            _mediaFileService.Update(files);

            // Update album ids for history
            var items = _historyService.GetByAlbum(local.Id, null);
            items.ForEach(x => x.AlbumId = target.Id);
            _historyService.UpdateMany(items);

            // Finally delete the old album
            _albumService.DeleteMany(new List<Album> { local });

            return UpdateResult.UpdateTags;
        }

        protected override Album GetEntityByForeignId(Album local)
        {
            return _albumService.FindById(local.ForeignAlbumId);
        }

        protected override void SaveEntity(Album local)
        {
            // Use UpdateMany to avoid firing the album edited event
            _albumService.UpdateMany(new List<Album> { local });
        }

        protected override void DeleteEntity(Album local, bool deleteFiles)
        {
            _albumService.DeleteAlbum(local.Id, true);
        }

        protected override List<AlbumRelease> GetRemoteChildren(Album remote)
        {
            return remote.AlbumReleases.Value.DistinctBy(m => m.ForeignReleaseId).ToList();
        }

        protected override List<AlbumRelease> GetLocalChildren(Album entity, List<AlbumRelease> remoteChildren)
        {
            var children = _releaseService.GetReleasesForRefresh(entity.Id,
                                                                 remoteChildren.Select(x => x.ForeignReleaseId)
                                                                 .Concat(remoteChildren.SelectMany(x => x.OldForeignReleaseIds)));

            // Make sure trackfiles point to the new album where we are grabbing a release from another album
            var files = new List<TrackFile>();
            foreach (var release in children.Where(x => x.AlbumId != entity.Id))
            {
                files.AddRange(_mediaFileService.GetFilesByRelease(release.Id));
            }

            files.ForEach(x => x.AlbumId = entity.Id);
            _mediaFileService.Update(files);

            return children;
        }

        protected override Tuple<AlbumRelease, List<AlbumRelease>> GetMatchingExistingChildren(List<AlbumRelease> existingChildren, AlbumRelease remote)
        {
            var existingChild = existingChildren.SingleOrDefault(x => x.ForeignReleaseId == remote.ForeignReleaseId);
            var mergeChildren = existingChildren.Where(x => remote.OldForeignReleaseIds.Contains(x.ForeignReleaseId)).ToList();
            return Tuple.Create(existingChild, mergeChildren);
        }

        protected override void PrepareNewChild(AlbumRelease child, Album entity)
        {
            child.AlbumId = entity.Id;
            child.Album = entity;
        }

        protected override void PrepareExistingChild(AlbumRelease local, AlbumRelease remote, Album entity)
        {
            local.AlbumId = entity.Id;
            local.Album = entity;

            remote.UseDbFieldsFrom(local);
        }

        protected override void AddChildren(List<AlbumRelease> children)
        {
            _releaseService.InsertMany(children);
        }

        private void MonitorSingleRelease(List<AlbumRelease> releases)
        {
            var monitored = releases.Where(x => x.Monitored).ToList();
            if (!monitored.Any())
            {
                monitored = releases;
            }

            var toMonitor = monitored.OrderByDescending(x => _mediaFileService.GetFilesByRelease(x.Id).Count)
                .ThenByDescending(x => x.TrackCount)
                .First();

            releases.ForEach(x => x.Monitored = false);
            toMonitor.Monitored = true;
        }

        protected override bool RefreshChildren(SortedChildren localChildren, List<AlbumRelease> remoteChildren, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            var refreshList = localChildren.All;

            // make sure only one of the releases ends up monitored
            localChildren.Old.ForEach(x => x.Monitored = false);
            MonitorSingleRelease(localChildren.Future);

            refreshList.ForEach(x => _logger.Trace($"release: {x} monitored: {x.Monitored}"));

            return _refreshAlbumReleaseService.RefreshEntityInfo(refreshList, remoteChildren, forceChildRefresh, forceUpdateFileTags);
        }

        protected override void PublishEntityUpdatedEvent(Album entity)
        {
            // Fetch fresh from DB so all lazy loads are available
            _eventAggregator.PublishEvent(new AlbumUpdatedEvent(_albumService.GetAlbum(entity.Id)));
        }

        public bool RefreshAlbumInfo(List<Album> albums, List<Album> remoteAlbums, bool forceAlbumRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            bool updated = false;

            HashSet<string> updatedMusicbrainzAlbums = null;

            if (lastUpdate.HasValue && lastUpdate.Value.AddDays(14) > DateTime.UtcNow)
            {
                updatedMusicbrainzAlbums = _albumInfo.GetChangedAlbums(lastUpdate.Value);
            }

            foreach (var album in albums)
            {
                if (forceAlbumRefresh ||
                    (updatedMusicbrainzAlbums == null && _checkIfAlbumShouldBeRefreshed.ShouldRefresh(album)) ||
                    (updatedMusicbrainzAlbums != null && updatedMusicbrainzAlbums.Contains(album.ForeignAlbumId)))
                {
                    updated |= RefreshAlbumInfo(album, remoteAlbums, forceUpdateFileTags);
                }
                else
                {
                    _logger.Debug("Skipping refresh of album: {0}", album.Title);
                }
            }

            return updated;
        }

        public bool RefreshAlbumInfo(Album album, List<Album> remoteAlbums, bool forceUpdateFileTags)
        {
            return RefreshEntityInfo(album, remoteAlbums, true, forceUpdateFileTags, null);
        }

        public void Execute(RefreshAlbumCommand message)
        {
            if (message.AlbumId.HasValue)
            {
                var album = _albumService.GetAlbum(message.AlbumId.Value);
                var artist = _artistService.GetArtistByMetadataId(album.ArtistMetadataId);
                var updated = RefreshAlbumInfo(album, null, false);
                if (updated)
                {
                    _eventAggregator.PublishEvent(new ArtistUpdatedEvent(artist));
                    _eventAggregator.PublishEvent(new AlbumUpdatedEvent(album));
                }
            }
        }
    }
}
