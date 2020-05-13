using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Exceptions;
using NzbDrone.Core.History;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Commands;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Profiles.Metadata;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.Books
{
    public class RefreshAuthorService : RefreshEntityServiceBase<Author, Book>,
        IExecute<RefreshAuthorCommand>,
        IExecute<BulkRefreshAuthorCommand>
    {
        private readonly IProvideAuthorInfo _authorInfo;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IMetadataProfileService _metadataProfileService;
        private readonly IRefreshBookService _refreshBookService;
        private readonly IRefreshSeriesService _refreshSeriesService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly IMediaFileService _mediaFileService;
        private readonly IHistoryService _historyService;
        private readonly IRootFolderService _rootFolderService;
        private readonly ICheckIfAuthorShouldBeRefreshed _checkIfAuthorShouldBeRefreshed;
        private readonly IConfigService _configService;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly Logger _logger;

        public RefreshAuthorService(IProvideAuthorInfo authorInfo,
                                    IAuthorService authorService,
                                    IAuthorMetadataService authorMetadataService,
                                    IBookService bookService,
                                    IMetadataProfileService metadataProfileService,
                                    IRefreshBookService refreshBookService,
                                    IRefreshSeriesService refreshSeriesService,
                                    IEventAggregator eventAggregator,
                                    IManageCommandQueue commandQueueManager,
                                    IMediaFileService mediaFileService,
                                    IHistoryService historyService,
                                    IRootFolderService rootFolderService,
                                    ICheckIfAuthorShouldBeRefreshed checkIfAuthorShouldBeRefreshed,
                                    IConfigService configService,
                                    IImportListExclusionService importListExclusionService,
                                    Logger logger)
        : base(logger, authorMetadataService)
        {
            _authorInfo = authorInfo;
            _authorService = authorService;
            _bookService = bookService;
            _metadataProfileService = metadataProfileService;
            _refreshBookService = refreshBookService;
            _refreshSeriesService = refreshSeriesService;
            _eventAggregator = eventAggregator;
            _commandQueueManager = commandQueueManager;
            _mediaFileService = mediaFileService;
            _historyService = historyService;
            _rootFolderService = rootFolderService;
            _checkIfAuthorShouldBeRefreshed = checkIfAuthorShouldBeRefreshed;
            _configService = configService;
            _importListExclusionService = importListExclusionService;
            _logger = logger;
        }

        private Author GetSkyhookData(string foreignId)
        {
            try
            {
                return _authorInfo.GetAuthorInfo(foreignId);
            }
            catch (AuthorNotFoundException)
            {
                _logger.Error($"Could not find author with id {foreignId}");
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
            return !_mediaFileService.GetFilesByAuthor(local.Id).Any();
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
                _logger.Warn(e, "Couldn't update author path for " + local.Path);
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

            // We know we need to update tags as author id has changed
            return UpdateResult.UpdateTags;
        }

        protected override UpdateResult MergeEntity(Author local, Author target, Author remote)
        {
            _logger.Warn($"Author {local} was replaced with {remote} because the original was a duplicate.");

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

            // move any books over to the new author and remove the local author
            var books = _bookService.GetBooksByAuthor(local.Id);
            books.ForEach(x => x.AuthorMetadataId = target.AuthorMetadataId);
            _bookService.UpdateMany(books);
            _authorService.DeleteAuthor(local.Id, false);

            // Update history entries to new id
            var items = _historyService.GetByAuthor(local.Id, null);
            items.ForEach(x => x.AuthorId = target.Id);
            _historyService.UpdateMany(items);

            // We know we need to update tags as author id has changed
            return UpdateResult.UpdateTags;
        }

        protected override Author GetEntityByForeignId(Author local)
        {
            return _authorService.FindById(local.ForeignAuthorId);
        }

        protected override void SaveEntity(Author local)
        {
            _authorService.UpdateAuthor(local);
        }

        protected override void DeleteEntity(Author local, bool deleteFiles)
        {
            _authorService.DeleteAuthor(local.Id, true);
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
            return _bookService.GetBooksForRefresh(entity.AuthorMetadataId,
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
            _bookService.InsertMany(children);
        }

        protected override bool RefreshChildren(SortedChildren localChildren, List<Book> remoteChildren, Author remoteData, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            return _refreshBookService.RefreshBookInfo(localChildren.All, remoteChildren, remoteData, forceChildRefresh, forceUpdateFileTags, lastUpdate);
        }

        protected override void PublishEntityUpdatedEvent(Author entity)
        {
            _eventAggregator.PublishEvent(new AuthorUpdatedEvent(entity));
        }

        protected override void PublishRefreshCompleteEvent(Author entity)
        {
            // little hack - trigger the series update here
            _refreshSeriesService.RefreshSeriesInfo(entity.AuthorMetadataId, entity.Series, entity, false, false, null);

            _eventAggregator.PublishEvent(new AuthorRefreshCompleteEvent(entity));
        }

        protected override void PublishChildrenUpdatedEvent(Author entity, List<Book> newChildren, List<Book> updateChildren)
        {
            _eventAggregator.PublishEvent(new BookInfoRefreshedEvent(entity, newChildren, updateChildren));
        }

        private void Rescan(List<int> authorIds, bool isNew, CommandTrigger trigger, bool infoUpdated)
        {
            var rescanAfterRefresh = _configService.RescanAfterRefresh;
            var shouldRescan = true;

            if (isNew)
            {
                _logger.Trace("Forcing rescan. Reason: New author added");
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
                // (but don't add new authors to reduce repeated searches against api)
                var folders = _rootFolderService.All().Select(x => x.Path).ToList();

                _commandQueueManager.Push(new RescanFoldersCommand(folders, FilterFilesType.Matched, false, authorIds));
            }
        }

        private void RefreshSelectedArtists(List<int> authorIds, bool isNew, CommandTrigger trigger)
        {
            var updated = false;
            var authors = _authorService.GetAuthors(authorIds);

            foreach (var author in authors)
            {
                try
                {
                    var data = GetSkyhookData(author.ForeignAuthorId);
                    updated |= RefreshEntityInfo(author, null, data, true, false, null);
                }
                catch (Exception e)
                {
                    _logger.Error(e, "Couldn't refresh info for {0}", author);
                }
            }

            Rescan(authorIds, isNew, trigger, updated);
        }

        public void Execute(BulkRefreshAuthorCommand message)
        {
            RefreshSelectedArtists(message.AuthorIds, message.AreNewAuthors, message.Trigger);
        }

        public void Execute(RefreshAuthorCommand message)
        {
            var trigger = message.Trigger;
            var isNew = message.IsNewAuthor;

            if (message.AuthorId.HasValue)
            {
                RefreshSelectedArtists(new List<int> { message.AuthorId.Value }, isNew, trigger);
            }
            else
            {
                var updated = false;
                var authors = _authorService.GetAllAuthors().OrderBy(c => c.Name).ToList();
                var authorIds = authors.Select(x => x.Id).ToList();

                var updatedGoodreadsAuthors = new HashSet<string>();

                if (message.LastExecutionTime.HasValue && message.LastExecutionTime.Value.AddDays(14) > DateTime.UtcNow)
                {
                    updatedGoodreadsAuthors = _authorInfo.GetChangedArtists(message.LastStartTime.Value);
                }

                foreach (var author in authors)
                {
                    var manualTrigger = message.Trigger == CommandTrigger.Manual;

                    if ((updatedGoodreadsAuthors == null && _checkIfAuthorShouldBeRefreshed.ShouldRefresh(author)) ||
                        (updatedGoodreadsAuthors != null && updatedGoodreadsAuthors.Contains(author.ForeignAuthorId)) ||
                        manualTrigger)
                    {
                        try
                        {
                            var data = GetSkyhookData(author.ForeignAuthorId);
                            updated |= RefreshEntityInfo(author, null, data, manualTrigger, false, message.LastStartTime);
                        }
                        catch (Exception e)
                        {
                            _logger.Error(e, "Couldn't refresh info for {0}", author);
                        }
                    }
                    else
                    {
                        _logger.Info("Skipping refresh of author: {0}", author.Name);
                    }
                }

                Rescan(authorIds, isNew, trigger, updated);
            }
        }
    }
}
