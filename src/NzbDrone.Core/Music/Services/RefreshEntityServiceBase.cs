using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;

namespace NzbDrone.Core.Music
{
    public abstract class RefreshEntityServiceBase<TEntity, TChild>
    {
        private readonly Logger _logger;
        private readonly IArtistMetadataService _artistMetadataService;

        protected RefreshEntityServiceBase(Logger logger,
                                           IArtistMetadataService artistMetadataService)
        {
            _logger = logger;
            _artistMetadataService = artistMetadataService;
        }

        public enum UpdateResult
        {
            None,
            Standard,
            UpdateTags
        }

        public class SortedChildren
        {
            public SortedChildren()
            {
                UpToDate = new List<TChild>();
                Added = new List<TChild>();
                Updated = new List<TChild>();
                Merged = new List<Tuple<TChild, TChild>>();
                Deleted = new List<TChild>();
            }

            public List<TChild> UpToDate { get; set; }
            public List<TChild> Added { get; set; }
            public List<TChild> Updated { get; set; }
            public List<Tuple<TChild, TChild>> Merged { get; set; }
            public List<TChild> Deleted { get; set; }

            public List<TChild> All => UpToDate.Concat(Added).Concat(Updated).Concat(Merged.Select(x => x.Item1)).Concat(Deleted).ToList();
            public List<TChild> Future => UpToDate.Concat(Added).Concat(Updated).ToList();
            public List<TChild> Old => Merged.Select(x => x.Item1).Concat(Deleted).ToList();
        }

        public class RemoteData
        {
            public TEntity Entity { get; set; }
            public List<ArtistMetadata> Metadata { get; set; }
        }

        protected virtual void LogProgress(TEntity local)
        {
        }

        protected abstract RemoteData GetRemoteData(TEntity local, List<TEntity> remote);

        protected virtual void EnsureNewParent(TEntity local, TEntity remote)
        {
        }

        protected abstract bool IsMerge(TEntity local, TEntity remote);

        protected virtual bool ShouldDelete(TEntity local)
        {
            return true;
        }

        protected abstract UpdateResult UpdateEntity(TEntity local, TEntity remote);

        protected virtual UpdateResult MoveEntity(TEntity local, TEntity remote)
        {
            return UpdateEntity(local, remote);
        }

        protected virtual UpdateResult MergeEntity(TEntity local, TEntity target, TEntity remote)
        {
            DeleteEntity(local, true);
            return UpdateResult.UpdateTags;
        }

        protected abstract TEntity GetEntityByForeignId(TEntity local);
        protected abstract void SaveEntity(TEntity local);
        protected abstract void DeleteEntity(TEntity local, bool deleteFiles);

        protected abstract List<TChild> GetRemoteChildren(TEntity remote);
        protected abstract List<TChild> GetLocalChildren(TEntity entity, List<TChild> remoteChildren);
        protected abstract Tuple<TChild, List<TChild>> GetMatchingExistingChildren(List<TChild> existingChildren, TChild remote);

        protected abstract void PrepareNewChild(TChild child, TEntity entity);
        protected abstract void PrepareExistingChild(TChild local, TChild remote, TEntity entity);
        protected abstract void AddChildren(List<TChild> children);
        protected abstract bool RefreshChildren(SortedChildren localChildren, List<TChild> remoteChildren, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate);

        protected virtual void PublishEntityUpdatedEvent(TEntity entity)
        {
        }

        protected virtual void PublishRefreshCompleteEvent(TEntity entity)
        {
        }

        protected virtual void PublishChildrenUpdatedEvent(TEntity entity, List<TChild> newChildren, List<TChild> updateChildren)
        {
        }

        public bool RefreshEntityInfo(TEntity local, List<TEntity> remoteList, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            bool updated = false;

            LogProgress(local);

            var data = GetRemoteData(local, remoteList);
            var remote = data.Entity;

            if (remote == null)
            {
                if (ShouldDelete(local))
                {
                    _logger.Warn($"{typeof(TEntity).Name} {local} not found in metadata and is being deleted");
                    DeleteEntity(local, true);
                    return false;
                }
                else
                {
                    _logger.Error($"{typeof(TEntity).Name} {local} was not found, it may have been removed from Metadata sources.");
                    return false;
                }
            }

            if (data.Metadata != null)
            {
                var metadataResult = UpdateArtistMetadata(data.Metadata);
                updated |= metadataResult >= UpdateResult.Standard;
                forceUpdateFileTags |= metadataResult == UpdateResult.UpdateTags;
            }

            // Validate that the parent object exists (remote data might specify a different one)
            EnsureNewParent(local, remote);

            UpdateResult result;
            if (IsMerge(local, remote))
            {
                // get entity we're merging into
                var target = GetEntityByForeignId(remote);

                if (target == null)
                {
                    _logger.Trace($"Moving {typeof(TEntity).Name} {local} to {remote}");
                    result = MoveEntity(local, remote);
                }
                else
                {
                    _logger.Trace($"Merging {typeof(TEntity).Name} {local} into {target}");
                    result = MergeEntity(local, target, remote);

                    // having merged local into target, do update for target using remote
                    local = target;
                }

                // Save the entity early so that children see the updated ids
                SaveEntity(local);
            }
            else
            {
                _logger.Trace($"Updating {typeof(TEntity).Name} {local}");
                result = UpdateEntity(local, remote);
            }

            updated |= result >= UpdateResult.Standard;
            forceUpdateFileTags |= result == UpdateResult.UpdateTags;

            _logger.Trace($"updated: {updated} forceUpdateFileTags: {forceUpdateFileTags}");

            var remoteChildren = GetRemoteChildren(remote);
            updated |= SortChildren(local, remoteChildren, forceChildRefresh, forceUpdateFileTags, lastUpdate);

            // Do this last so entity only marked as refreshed if refresh of children completed successfully
            _logger.Trace($"Saving {typeof(TEntity).Name} {local}");
            SaveEntity(local);

            if (updated)
            {
                PublishEntityUpdatedEvent(local);
            }

            PublishRefreshCompleteEvent(local);

            _logger.Debug($"Finished {typeof(TEntity).Name} refresh for {local}");

            return updated;
        }

        public bool RefreshEntityInfo(List<TEntity> localList, List<TEntity> remoteList, bool forceChildRefresh, bool forceUpdateFileTags)
        {
            bool updated = false;
            foreach (var entity in localList)
            {
                updated |= RefreshEntityInfo(entity, remoteList, forceChildRefresh, forceUpdateFileTags, null);
            }

            return updated;
        }

        public UpdateResult UpdateArtistMetadata(List<ArtistMetadata> data)
        {
            var remoteMetadata = data.DistinctBy(x => x.ForeignArtistId).ToList();
            var updated = _artistMetadataService.UpsertMany(remoteMetadata);
            return updated ? UpdateResult.UpdateTags : UpdateResult.None;
        }

        protected bool SortChildren(TEntity entity, List<TChild> remoteChildren, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            // Get existing children (and children to be) from the database
            var localChildren = GetLocalChildren(entity, remoteChildren);

            var sortedChildren = new SortedChildren();
            sortedChildren.Deleted.AddRange(localChildren);

            // Cycle through children
            foreach (var remoteChild in remoteChildren)
            {
                // Check for child in existing children, if not set properties and add to new list
                var tuple = GetMatchingExistingChildren(localChildren, remoteChild);
                var existingChild = tuple.Item1;
                var mergedChildren = tuple.Item2;

                if (existingChild != null)
                {
                    sortedChildren.Deleted.Remove(existingChild);

                    PrepareExistingChild(existingChild, remoteChild, entity);

                    if (existingChild.Equals(remoteChild))
                    {
                        sortedChildren.UpToDate.Add(existingChild);
                    }
                    else
                    {
                        sortedChildren.Updated.Add(existingChild);
                    }

                    // note the children that are going to be merged into existingChild
                    foreach (var child in mergedChildren)
                    {
                        sortedChildren.Merged.Add(Tuple.Create(child, existingChild));
                        sortedChildren.Deleted.Remove(child);
                    }
                }
                else
                {
                    PrepareNewChild(remoteChild, entity);
                    sortedChildren.Added.Add(remoteChild);

                    // note the children that will be merged into remoteChild (once added)
                    foreach (var child in mergedChildren)
                    {
                        sortedChildren.Merged.Add(Tuple.Create(child, remoteChild));
                        sortedChildren.Deleted.Remove(child);
                    }
                }
            }

            _logger.Debug("{0} {1} {2}s up to date. Adding {3}, Updating {4}, Merging {5}, Deleting {6}.",
                          entity,
                          sortedChildren.UpToDate.Count,
                          typeof(TChild).Name.ToLower(),
                          sortedChildren.Added.Count,
                          sortedChildren.Updated.Count,
                          sortedChildren.Merged.Count,
                          sortedChildren.Deleted.Count);

            // Add in the new children (we have checked that foreign IDs don't clash)
            AddChildren(sortedChildren.Added);

            // now trigger updates
            var updated = RefreshChildren(sortedChildren, remoteChildren, forceChildRefresh, forceUpdateFileTags, lastUpdate);

            PublishChildrenUpdatedEvent(entity, sortedChildren.Added, sortedChildren.Updated);
            return updated;
        }
    }
}
