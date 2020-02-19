using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public interface IRefreshAlbumReleaseService
    {
        bool RefreshEntityInfo(AlbumRelease entity, List<AlbumRelease> remoteEntityList, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate);
        bool RefreshEntityInfo(List<AlbumRelease> releases, List<AlbumRelease> remoteEntityList, bool forceChildRefresh, bool forceUpdateFileTags);
    }

    public class RefreshAlbumReleaseService : RefreshEntityServiceBase<AlbumRelease, Track>, IRefreshAlbumReleaseService
    {
        private readonly IReleaseService _releaseService;
        private readonly IRefreshTrackService _refreshTrackService;
        private readonly ITrackService _trackService;

        public RefreshAlbumReleaseService(IReleaseService releaseService,
                                          IArtistMetadataService artistMetadataService,
                                          IRefreshTrackService refreshTrackService,
                                          ITrackService trackService,
                                          Logger logger)
        : base(logger, artistMetadataService)
        {
            _releaseService = releaseService;
            _trackService = trackService;
            _refreshTrackService = refreshTrackService;
        }

        protected override RemoteData GetRemoteData(AlbumRelease local, List<AlbumRelease> remote)
        {
            var result = new RemoteData();
            result.Entity = remote.SingleOrDefault(x => x.ForeignReleaseId == local.ForeignReleaseId || x.OldForeignReleaseIds.Contains(local.ForeignReleaseId));
            return result;
        }

        protected override bool IsMerge(AlbumRelease local, AlbumRelease remote)
        {
            return local.ForeignReleaseId != remote.ForeignReleaseId;
        }

        protected override UpdateResult UpdateEntity(AlbumRelease local, AlbumRelease remote)
        {
            if (local.Equals(remote))
            {
                return UpdateResult.None;
            }

            local.UseMetadataFrom(remote);

            return UpdateResult.UpdateTags;
        }

        protected override AlbumRelease GetEntityByForeignId(AlbumRelease local)
        {
            return _releaseService.GetReleaseByForeignReleaseId(local.ForeignReleaseId);
        }

        protected override void SaveEntity(AlbumRelease local)
        {
            _releaseService.UpdateMany(new List<AlbumRelease> { local });
        }

        protected override void DeleteEntity(AlbumRelease local, bool deleteFiles)
        {
            _releaseService.DeleteMany(new List<AlbumRelease> { local });
        }

        protected override List<Track> GetRemoteChildren(AlbumRelease remote)
        {
            return remote.Tracks.Value.DistinctBy(m => m.ForeignTrackId).ToList();
        }

        protected override List<Track> GetLocalChildren(AlbumRelease entity, List<Track> remoteChildren)
        {
            return _trackService.GetTracksForRefresh(entity.Id,
                                                     remoteChildren.Select(x => x.ForeignTrackId)
                                                         .Concat(remoteChildren.SelectMany(x => x.OldForeignTrackIds)));
        }

        protected override Tuple<Track, List<Track>> GetMatchingExistingChildren(List<Track> existingChildren, Track remote)
        {
            var existingChild = existingChildren.SingleOrDefault(x => x.ForeignTrackId == remote.ForeignTrackId);
            var mergeChildren = existingChildren.Where(x => remote.OldForeignTrackIds.Contains(x.ForeignTrackId)).ToList();
            return Tuple.Create(existingChild, mergeChildren);
        }

        protected override void PrepareNewChild(Track child, AlbumRelease entity)
        {
            child.AlbumReleaseId = entity.Id;
            child.AlbumRelease = entity;
            child.ArtistMetadataId = child.ArtistMetadata.Value.Id;

            // make sure title is not null
            child.Title = child.Title ?? "Unknown";
        }

        protected override void PrepareExistingChild(Track local, Track remote, AlbumRelease entity)
        {
            local.AlbumRelease = entity;
            local.AlbumReleaseId = entity.Id;
            local.ArtistMetadataId = remote.ArtistMetadata.Value.Id;
            remote.Id = local.Id;
            remote.TrackFileId = local.TrackFileId;
            remote.AlbumReleaseId = local.AlbumReleaseId;
            remote.ArtistMetadataId = local.ArtistMetadataId;
        }

        protected override void AddChildren(List<Track> children)
        {
            _trackService.InsertMany(children);
        }

        protected override bool RefreshChildren(SortedChildren localChildren, List<Track> remoteChildren, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            return _refreshTrackService.RefreshTrackInfo(localChildren.Added, localChildren.Updated, localChildren.Merged, localChildren.Deleted, localChildren.UpToDate, remoteChildren, forceUpdateFileTags);
        }
    }
}
