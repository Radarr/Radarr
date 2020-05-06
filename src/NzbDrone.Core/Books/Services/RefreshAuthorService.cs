using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.History;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Music.Commands;
using NzbDrone.Core.Music.Events;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Music
{
    public class RefreshArtistService : RefreshEntityServiceBase<Author, Book>,
        IExecute<RefreshArtistCommand>,
        IExecute<BulkRefreshArtistCommand>
    {
        private readonly IProvideAuthorInfo _artistInfo;
        private readonly IArtistService _artistService;
        private readonly IAlbumService _albumService;
        private readonly IMetadataProfileService _metadataProfileService;
        private readonly IRefreshAlbumService _refreshAlbumService;
        private readonly IRefreshSeriesService _refreshSeriesService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IMediaFileService _mediaFileService;
        private readonly IHistoryService _historyService;
        private readonly IRootFolderService _rootFolderService;
        private readonly ICheckIfArtistShouldBeRefreshed _checkIfArtistShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly Logger _logger;

        public RefreshArtistService(IProvideAuthorInfo artistInfo,
                                    IArtistService artistService,
                                    IArtistMetadataService artistMetadataService,
                                    IAlbumService albumService,
                                    IMetadataProfileService metadataProfileService,
                                    IRefreshAlbumService refreshAlbumService,
                                    IRefreshSeriesService refreshSeriesService,
                                    IEventAggregator eventAggregator,
                                    IManageCommandQueue commandQueueManager,
                                    IMediaFileService mediaFileService,
                                    IHistoryService historyService,
                                    IRootFolderService rootFolderService,
                                    ICheckIfArtistShouldBeRefreshed checkIfArtistShouldBeRefreshed,
                                    IConfigService configService,
                                    IImportListExclusionService importListExclusionService,
                                    Logger logger)
        : base(logger, artistMetadataService)
        {
            _artistInfo = artistInfo;
            _artistService = artistService;
            _albumService = albumService;
            _metadataProfileService = metadataProfileService;
            _refreshAlbumService = refreshAlbumService;
            _refreshSeriesService = refreshSeriesService;
            _eventAggregator = eventAggregator;
            _commandQueueManager = commandQueueManager;
            _mediaFileService = mediaFileService;
            _historyService = historyService;
            _rootFolderService = rootFolderService;
            _checkIfArtistShouldBeRefreshed = checkIfArtistShouldBeRefreshed;
            _configService = configService;
            _importListExclusionService = importListExclusionService;
            _logger = logger;
        }

        private Author GetSkyhookData(string foreignId)
        {
            try
            {
                return _artistInfo.GetAuthorInfo(foreignId);
            }
            catch (ArtistNotFoundException)
            {
                _logger.Error($"Could not find artist with id {foreignId}");
            }

            return null;
        }

        protected override RemoteData GetRemoteData(Author local, List<Author> remote, Author data)
        {
            var result = new RemoteData();

            if (data != null)
            {
                result.Entity = data;
                result.Metadata = new List<AuthorMetadata> { data.Metadata.Value };
            }

            return result;
        }

        protected override bool ShouldDelete(Author local)
        {
            return !_mediaFileService.GetFilesByArtist(local.Id).Any();
        }

        protected override void LogProgress(Author local)
        {
            _logger.ProgressInfo("Updating Info for {0}", local.Name);
        }

        protected override bool IsMerge(Author local, Author remote)
        {
            _logger.Trace($"local: {local.AuthorMetadataId} remote: {remote.Metadata.Value.Id}");
            return local.AuthorMetadataId != remote.Metadata.Value.Id;
        }

        protected override UpdateResult UpdateEntity(Author local, Author remote)
        {
            var result = UpdateResult.None;

            if (!local.Metadata.Value.Equals(remote.Metadata.Value))
            {
                result = UpdateResult.UpdateTags;
            }

            local.UseMetadataFrom(remote);
            local.Metadata = remote.Metadata;
            local.Series = remote.Series.Value;
            local.LastInfoSync = DateTime.UtcNow;

            try
            {
                local.Path = new DirectoryInfo(local.Path).FullName;
                local.Path = local.Path.GetActualCasing();
            }
            catch (Exception e)
            {
                _logger.Warn(e, "Couldn't update artist path for " + local.Path);
            }

            return result;
        }

        protected override UpdateResult MoveEntity(Author local, Author remote)
        {
            _logger.Debug($"Updating foreign id for {local} to {remote}");

            // We are moving from one metadata to another (will already have been poplated)
            local.AuthorMetadataId = remote.Metadata.Value.Id;
            local.Metadata = remote.Metadata.Value;

            // Update list exclusion if one exists
            var importExclusion = _importListExclusionService.FindByForeignId(local.Metadata.Value.ForeignAuthorId);

            if (importExclusion != null)
            {
                importExclusion.ForeignId = remote.Metadata.Value.ForeignAuthorId;
                _importListExclusionService.Update(importExclusion);
            }

            // Do the standard update
            UpdateEntity(local, remote);

            // We know we need to update tags as artist id has changed
            return UpdateResult.UpdateTags;
        }

        protected override UpdateResult MergeEntity(Author local, Author target, Author remote)
        {
            _logger.Warn($"Artist {local} was replaced with {remote} because the original was a duplicate.");

            // Update list exclusion if one exists
            var importExclusionLocal = _importListExclusionService.FindByForeignId(local.Metadata.Value.ForeignAuthorId);

            if (importExclusionLocal != null)
            {
                var importExclusionTarget = _importListExclusionService.FindByForeignId(target.Metadata.Value.ForeignAuthorId);
                if (importExclusionTarget == null)
                {
                    importExclusionLocal.ForeignId = remote.Metadata.Value.ForeignAuthorId;
                    _importListExclusionService.Update(importExclusionLocal);
                }
            }

            // move any albums over to the new artist and remove the local artist
            var albums = _albumService.GetAlbumsByArtist(local.Id);
            albums.ForEach(x => x.AuthorMetadataId = target.AuthorMetadataId);
            _albumService.UpdateMany(albums);
            _artistService.DeleteArtist(local.Id, false);

            // Update history entries to new id
            var items = _historyService.GetByArtist(local.Id, null);
            items.ForEach(x => x.AuthorId = target.Id);
            _historyService.UpdateMany(items);

            // We know we need to update tags as artist id has changed
            return UpdateResult.UpdateTags;
        }

        protected override Author GetEntityByForeignId(Author local)
        {
            return _artistService.FindById(local.ForeignAuthorId);
        }

        protected override void SaveEntity(Author local)
        {
            _artistService.UpdateArtist(local);
        }

        protected override void DeleteEntity(Author local, bool deleteFiles)
        {
            _artistService.DeleteArtist(local.Id, true);
        }

        protected override List<Book> GetRemoteChildren(Author local, Author remote)
        {
            var filtered = _metadataProfileService.FilterBooks(remote, local.MetadataProfileId);

            var all = filtered.DistinctBy(m => m.ForeignBookId).ToList();
            var ids = all.Select(x => x.ForeignBookId).ToList();
            var excluded = _importListExclusionService.FindByForeignId(ids).Select(x => x.ForeignId).ToList();
            return all.Where(x => !excluded.Contains(x.ForeignBookId)).ToList();
        }

        protected override List<Book> GetLocalChildren(Author entity, List<Book> remoteChildren)
        {
            return _albumService.GetAlbumsForRefresh(entity.AuthorMetadataId,
                                                     remoteChildren.Select(x => x.ForeignBookId));
        }

        protected override Tuple<Book, List<Book>> GetMatchingExistingChildren(List<Book> existingChildren, Book remote)
        {
            var existingChild = existingChildren.SingleOrDefault(x => x.ForeignBookId == remote.ForeignBookId);
            var mergeChildren = new List<Book>();
            return Tuple.Create(existingChild, mergeChildren);
        }

        protected override void PrepareNewChild(Book child, Author entity)
        {
            child.Author = entity;
            child.AuthorMetadata = entity.Metadata.Value;
            child.AuthorMetadataId = entity.Metadata.Value.Id;
            child.Added = DateTime.UtcNow;
            child.LastInfoSync = DateTime.MinValue;
            child.Monitored = entity.Monitored;
        }

        protected override void PrepareExistingChild(Book local, Book remote, Author entity)
        {
            local.Author = entity;
            local.AuthorMetadata = entity.Metadata.Value;
            local.AuthorMetadataId = entity.Metadata.Value.Id;

            remote.UseDbFieldsFrom(local);
        }

        protected override void AddChildren(List<Book> children)
        {
            _albumService.InsertMany(children);
        }

        protected override bool RefreshChildren(SortedChildren localChildren, List<Book> remoteChildren, Author remoteData, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            return _refreshAlbumService.RefreshAlbumInfo(localChildren.All, remoteChildren, remoteData, forceChildRefresh, forceUpdateFileTags, lastUpdate);
        }

        protected override void PublishEntityUpdatedEvent(Author entity)
        {
            _eventAggregator.PublishEvent(new ArtistUpdatedEvent(entity));
        }

        protected override void PublishRefreshCompleteEvent(Author entity)
        {
            // little hack - trigger the series update here
            _refreshSeriesService.RefreshSeriesInfo(entity.AuthorMetadataId, entity.Series, entity, false, false, null);

            _eventAggregator.PublishEvent(new ArtistRefreshCompleteEvent(entity));
        }

        protected override void PublishChildrenUpdatedEvent(Author entity, List<Book> newChildren, List<Book> updateChildren)
        {
            _eventAggregator.PublishEvent(new AlbumInfoRefreshedEvent(entity, newChildren, updateChildren));
        }

        private void Rescan(List<int> authorIds, bool isNew, CommandTrigger trigger, bool infoUpdated)
        {
            var rescanAfterRefresh = _configService.RescanAfterRefresh;
            var shouldRescan = true;

            if (isNew)
            {
                _logger.Trace("Forcing rescan. Reason: New artist added");
                shouldRescan = true;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.Never)
            {
                _logger.Trace("Skipping rescan. Reason: never rescan after refresh");
                shouldRescan = false;
            }
            else if (rescanAfterRefresh == RescanAfterRefreshType.AfterManual && trigger != CommandTrigger.Manual)
            {
                _logger.Trace("Skipping rescan. Reason: not after automatic refreshes");
                shouldRescan = false;
            }
            else if (!infoUpdated)
            {
                _logger.Trace("Skipping rescan. Reason: no metadata updated");
                shouldRescan = false;
            }

            if (shouldRescan)
            {
                // some metadata has updated so rescan unmatched
                // (but don't add new artists to reduce repeated searches against api)
                var folders = _rootFolderService.All().Select(x => x.Path).ToList();

                _commandQueueManager.Push(new RescanFoldersCommand(folders, FilterFilesType.Matched, false, authorIds));
            }
        }

        private void RefreshSelectedArtists(List<int> authorIds, bool isNew, CommandTrigger trigger)
        {
            var updated = false;
            var artists = _artistService.GetArtists(authorIds);

            foreach (var artist in artists)
            {
                try
                {
                    var data = GetSkyhookData(artist.ForeignAuthorId);
                    updated |= RefreshEntityInfo(artist, null, data, true, false, null);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't refresh info for {0}", artist);
                }
            }

            Rescan(authorIds, isNew, trigger, updated);
        }

        public void Execute(BulkRefreshArtistCommand message)
        {
            RefreshSelectedArtists(message.AuthorIds, message.AreNewArtists, message.Trigger);
        }

        public void Execute(RefreshArtistCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewArtist;

            if (message.AuthorId.HasValue)
            {
                RefreshSelectedArtists(new List<int> { message.AuthorId.Value }, isNew, trigger);
            }
            else
            {
                var updated = false;
                var artists = _artistService.GetAllArtists().OrderBy(c => c.Name).ToList();
                var authorIds = artists.Select(x => x.Id).ToList();

                var updatedMusicbrainzArtists = new HashSet<string>();

                if (message.LastExecutionTime.HasValue && message.LastExecutionTime.Value.AddDays(14) > DateTime.UtcNow)
                {
                    updatedMusicbrainzArtists = _artistInfo.GetChangedArtists(message.LastStartTime.Value);
                }

                foreach (var artist in artists)
                {
                    var manualTrigger = message.Trigger == CommandTrigger.Manual;

                    if ((updatedMusicbrainzArtists == null && _checkIfArtistShouldBeRefreshed.ShouldRefresh(artist)) ||
                        (updatedMusicbrainzArtists != null && updatedMusicbrainzArtists.Contains(artist.ForeignAuthorId)) ||
                        manualTrigger)
                    {
                        try
                        {
                            var data = GetSkyhookData(artist.ForeignAuthorId);
                            updated |= RefreshEntityInfo(artist, null, data, manualTrigger, false, message.LastStartTime);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", artist);
                        }
                    }
                    else
                    {
                        _logger.Info("Skipping refresh of artist: {0}", artist.Name);
                    }
                }

                Rescan(authorIds, isNew, trigger, updated);
            }
        }
    }
}
