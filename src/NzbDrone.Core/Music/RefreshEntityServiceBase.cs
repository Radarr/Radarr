using NLog;
using NzbDrone.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NzbDrone.Core.Music
{
    public abstract class RefreshEntityServiceBase<Entity, Child>
    {
        private readonly Logger _logger;
        private readonly IArtistMetadataRepository _artistMetadataRepository;

        public RefreshEntityServiceBase(Logger logger,
                                        IArtistMetadataRepository artistMetadataRepository)
        {
            _logger = logger;
            _artistMetadataRepository = artistMetadataRepository;
        }
        
        public enum UpdateResult
        {
            None,
            Standard,
            UpdateTags
        };

        public class SortedChildren
        {
            public SortedChildren()
            {
                UpToDate = new List<Child>();
                Added = new List<Child>();
                Updated = new List<Child>();
                Merged = new List<Tuple<Child, Child> >();
                Deleted = new List<Child>();
            }

            public List<Child> UpToDate { get; set; }
            public List<Child> Added { get; set; }
            public List<Child> Updated { get; set; }
            public List<Tuple<Child, Child> > Merged { get; set; }
            public List<Child> Deleted { get; set; }
            
            public List<Child> All => UpToDate.Concat(Added).Concat(Updated).Concat(Merged.Select(x => x.Item1)).Concat(Deleted).ToList();
            public List<Child> Future => UpToDate.Concat(Added).Concat(Updated).ToList();
            public List<Child> Old => Merged.Select(x => x.Item1).Concat(Deleted).ToList();
        }

        public class RemoteData
        {
            public Entity Entity { get; set; }
            public List<ArtistMetadata> Metadata { get; set; }
        }

        protected virtual void LogProgress(Entity local)
        {
        }
        
        protected abstract RemoteData GetRemoteData(Entity local, List<Entity> remote);

        protected virtual void EnsureNewParent(Entity local, Entity remote)
        {
            return;
        }
        
        protected abstract bool IsMerge(Entity local, Entity remote);
        
        protected virtual bool ShouldDelete(Entity local)
        {
            return true;
        }
        
        protected abstract UpdateResult UpdateEntity(Entity local, Entity remote);
        
        protected virtual UpdateResult MoveEntity(Entity local, Entity remote)
        {
            return UpdateEntity(local, remote);
        }
        
        protected virtual UpdateResult MergeEntity(Entity local, Entity target, Entity remote)
        {
            DeleteEntity(local, true);
            return UpdateResult.UpdateTags;
        }

        protected abstract Entity GetEntityByForeignId(Entity local);
        protected abstract void SaveEntity(Entity local);
        protected abstract void DeleteEntity(Entity local, bool deleteFiles);
        
        protected abstract List<Child> GetRemoteChildren(Entity remote);
        protected abstract List<Child> GetLocalChildren(Entity entity, List<Child> remoteChildren);
        protected abstract Tuple<Child, List<Child> > GetMatchingExistingChildren(List<Child> existingChildren, Child remote);
        
        protected abstract void PrepareNewChild(Child remoteChild, Entity entity);
        protected abstract void PrepareExistingChild(Child existingChild, Child remoteChild, Entity entity);
        protected abstract void AddChildren(List<Child> children);
        protected abstract bool RefreshChildren(SortedChildren localChildren, List<Child> remoteChildren, bool forceChildRefresh, bool forceUpdateFileTags);

        protected virtual void PublishEntityUpdatedEvent(Entity entity)
        {
        }
        
        protected virtual void PublishChildrenUpdatedEvent(Entity entity, List<Child> newChildren, List<Child> updateChildren)
        {
        }

        public bool RefreshEntityInfo(Entity local, List<Entity> remoteList, bool forceChildRefresh, bool forceUpdateFileTags)
        {
            bool updated = false;
            
            LogProgress(local);
            
            var data = GetRemoteData(local, remoteList);
            var remote = data.Entity;

            if (remote == null)
            {
                if (ShouldDelete(local))
                {
                    _logger.Warn($"{typeof(Entity).Name} {local} not found in metadata and is being deleted");
                    DeleteEntity(local, true);
                    return false;
                }
                else
                {
                    _logger.Error($"{typeof(Entity).Name} {local} was not found, it may have been removed from Metadata sources.");
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
                    _logger.Trace($"Moving {typeof(Entity).Name} {local} to {remote}");
                    result = MoveEntity(local, remote);
                }
                else
                {
                    _logger.Trace($"Merging {typeof(Entity).Name} {local} into {target}");
                    result = MergeEntity(local, target, remote);

                    // having merged local into target, do update for target using remote
                    local = target;
                }

                // Save the entity early so that children see the updated ids
                SaveEntity(local);
            }
            else
            {
                _logger.Trace($"Updating {typeof(Entity).Name} {local}");
                result = UpdateEntity(local, remote);
            }

            updated |= result >= UpdateResult.Standard;
            forceUpdateFileTags |= result == UpdateResult.UpdateTags;

            _logger.Trace($"updated: {updated} forceUpdateFileTags: {forceUpdateFileTags}");
            
            var remoteChildren = GetRemoteChildren(remote);
            updated |= SortChildren(local, remoteChildren, forceChildRefresh, forceUpdateFileTags);

            // Do this last so entity only marked as refreshed if refresh of children completed successfully
            _logger.Trace($"Saving {typeof(Entity).Name} {local}");
            SaveEntity(local);

            if (updated)
            {
                PublishEntityUpdatedEvent(local);
            }

            _logger.Debug($"Finished {typeof(Entity).Name} refresh for {local}");

            return updated;
        }

        public UpdateResult UpdateArtistMetadata(List<ArtistMetadata> data)
        {
            var remoteMetadata = data.DistinctBy(x => x.ForeignArtistId).ToList();
            var updated = _artistMetadataRepository.UpsertMany(data);
            return updated ? UpdateResult.UpdateTags : UpdateResult.None;
        }

        public bool RefreshEntityInfo(List<Entity> localList, List<Entity> remoteList, bool forceChildRefresh, bool forceUpdateFileTags)
        {
            bool updated = false;
            foreach (var entity in localList)
            {
                updated |= RefreshEntityInfo(entity, remoteList, forceChildRefresh, forceUpdateFileTags);
            }
            return updated;
        }

        protected bool SortChildren(Entity entity, List<Child> remoteChildren, bool forceChildRefresh, bool forceUpdateFileTags)
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
                        sortedChildren.Merged.Add(Tuple.Create(child, existingChild));
                        sortedChildren.Deleted.Remove(child);
                    }

                }
            }

            _logger.Debug("{0} {1} {2}s up to date. Adding {3}, Updating {4}, Merging {5}, Deleting {6}.",
                          entity, sortedChildren.UpToDate.Count, typeof(Child).Name.ToLower(), 
                          sortedChildren.Added.Count, sortedChildren.Updated.Count, sortedChildren.Merged.Count, sortedChildren.Deleted.Count);

            // Add in the new children (we have checked that foreign IDs don't clash)
            AddChildren(sortedChildren.Added);

            // now trigger updates
            var updated = RefreshChildren(sortedChildren, remoteChildren, forceChildRefresh, forceUpdateFileTags);
            
            PublishChildrenUpdatedEvent(entity, sortedChildren.Added, sortedChildren.Updated);
            return updated;
        }
    }
}
