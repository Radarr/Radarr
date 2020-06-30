using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.History;
using NzbDrone.Core.MediaCover;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;

namespace NzbDrone.Core.Books
{
    public interface IRefreshBookService
    {
        bool RefreshBookInfo(Book book, List<Book> remoteBooks, Author remoteData, bool forceUpdateFileTags);
        bool RefreshBookInfo(List<Book> books, List<Book> remoteBooks, Author remoteData, bool forceBookRefresh, bool forceUpdateFileTags, DateTime? lastUpdate);
    }

    public class RefreshBookService : RefreshEntityServiceBase<Book, Edition>, IRefreshBookService
    {
        private readonly IBookService _bookService;
        private readonly IAuthorService _authorService;
        private readonly IAddAuthorService _addAuthorService;
        private readonly IEditionService _editionService;
        private readonly IProvideBookInfo _bookInfo;
        private readonly IRefreshEditionService _refreshEditionService;
        private readonly IMediaFileService _mediaFileService;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ICheckIfBookShouldBeRefreshed _checkIfBookShouldBeRefreshed;
        private readonly IMapCoversToLocal _mediaCoverService;
        private readonly Logger _logger;

        public RefreshBookService(IBookService bookService,
                                  IAuthorService authorService,
                                  IAddAuthorService addAuthorService,
                                  IEditionService editionService,
                                  IAuthorMetadataService authorMetadataService,
                                  IProvideBookInfo bookInfo,
                                  IRefreshEditionService refreshEditionService,
                                  IMediaFileService mediaFileService,
                                  IHistoryService historyService,
                                  IEventAggregator eventAggregator,
                                  ICheckIfBookShouldBeRefreshed checkIfBookShouldBeRefreshed,
                                  IMapCoversToLocal mediaCoverService,
                                  Logger logger)
        : base(logger, authorMetadataService)
        {
            _bookService = bookService;
            _authorService = authorService;
            _addAuthorService = addAuthorService;
            _editionService = editionService;
            _bookInfo = bookInfo;
            _refreshEditionService = refreshEditionService;
            _mediaFileService = mediaFileService;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _checkIfBookShouldBeRefreshed = checkIfBookShouldBeRefreshed;
            _mediaCoverService = mediaCoverService;
            _logger = logger;
        }

        protected override RemoteData GetRemoteData(Book local, List<Book> remote, Author data)
        {
            var result = new RemoteData();

            var book = remote.SingleOrDefault(x => x.ForeignBookId == local.ForeignBookId);

            if (book == null && ShouldDelete(local))
            {
                return result;
            }

            if (book == null)
            {
                book = data.Books.Value.SingleOrDefault(x => x.ForeignBookId == local.ForeignBookId);
            }

            result.Entity = book;
            if (result.Entity != null)
            {
                result.Entity.Id = local.Id;
            }

            return result;
        }

        protected override void EnsureNewParent(Book local, Book remote)
        {
            // Make sure the appropriate author exists (it could be that an book changes parent)
            // The authorMetadata entry will be in the db but make sure a corresponding author is too
            // so that the book doesn't just disappear.

            // TODO filter by metadata id before hitting database
            _logger.Trace($"Ensuring parent author exists [{remote.AuthorMetadata.Value.ForeignAuthorId}]");

            var newAuthor = _authorService.FindById(remote.AuthorMetadata.Value.ForeignAuthorId);

            if (newAuthor == null)
            {
                var oldAuthor = local.Author.Value;
                var addArtist = new Author
                {
                    Metadata = remote.AuthorMetadata.Value,
                    MetadataProfileId = oldAuthor.MetadataProfileId,
                    QualityProfileId = oldAuthor.QualityProfileId,
                    RootFolderPath = oldAuthor.RootFolderPath,
                    Monitored = oldAuthor.Monitored,
                    Tags = oldAuthor.Tags
                };
                _logger.Debug($"Adding missing parent author {addArtist}");
                _addAuthorService.AddAuthor(addArtist);
            }
        }

        protected override bool ShouldDelete(Book local)
        {
            // not manually added and has no files
            return local.AddOptions.AddType != BookAddType.Manual &&
                !_mediaFileService.GetFilesByBook(local.Id).Any();
        }

        protected override void LogProgress(Book local)
        {
            _logger.ProgressInfo("Updating Info for {0}", local.Title);
        }

        protected override bool IsMerge(Book local, Book remote)
        {
            return local.ForeignBookId != remote.ForeignBookId;
        }

        protected override UpdateResult UpdateEntity(Book local, Book remote)
        {
            UpdateResult result;

            remote.UseDbFieldsFrom(local);

            if (local.Title != (remote.Title ?? "Unknown") ||
                local.ForeignBookId != remote.ForeignBookId ||
                local.AuthorMetadata.Value.ForeignAuthorId != remote.AuthorMetadata.Value.ForeignAuthorId)
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
            // if (remote.Images.Any() && !local.Images.SequenceEqual(remote.Images))
            // {
            //     _mediaCoverService.EnsureAlbumCovers(remote);
            //     result = UpdateResult.UpdateTags;
            // }
            local.UseMetadataFrom(remote);

            local.AuthorMetadataId = remote.AuthorMetadata.Value.Id;
            local.LastInfoSync = DateTime.UtcNow;

            return result;
        }

        protected override UpdateResult MergeEntity(Book local, Book target, Book remote)
        {
            _logger.Warn($"Book {local} was merged with {remote} because the original was a duplicate.");

            // Update book ids for trackfiles
            var files = _mediaFileService.GetFilesByBook(local.Id);
            files.ForEach(x => x.EditionId = target.Id);
            _mediaFileService.Update(files);

            // Update book ids for history
            var items = _historyService.GetByBook(local.Id, null);
            items.ForEach(x => x.BookId = target.Id);
            _historyService.UpdateMany(items);

            // Finally delete the old book
            _bookService.DeleteMany(new List<Book> { local });

            return UpdateResult.UpdateTags;
        }

        protected override Book GetEntityByForeignId(Book local)
        {
            return _bookService.FindById(local.ForeignBookId);
        }

        protected override void SaveEntity(Book local)
        {
            // Use UpdateMany to avoid firing the book edited event
            _bookService.UpdateMany(new List<Book> { local });
        }

        protected override void DeleteEntity(Book local, bool deleteFiles)
        {
            _bookService.DeleteBook(local.Id, true);
        }

        protected override List<Edition> GetRemoteChildren(Book local, Book remote)
        {
            return remote.Editions.Value.DistinctBy(m => m.ForeignEditionId).ToList();
        }

        protected override List<Edition> GetLocalChildren(Book entity, List<Edition> remoteChildren)
        {
            return _editionService.GetEditionsForRefresh(entity.Id, remoteChildren.Select(x => x.ForeignEditionId));
        }

        protected override Tuple<Edition, List<Edition>> GetMatchingExistingChildren(List<Edition> existingChildren, Edition remote)
        {
            var existingChild = existingChildren.SingleOrDefault(x => x.ForeignEditionId == remote.ForeignEditionId);
            return Tuple.Create(existingChild, new List<Edition>());
        }

        protected override void PrepareNewChild(Edition child, Book entity)
        {
            child.BookId = entity.Id;
            child.Book = entity;
        }

        protected override void PrepareExistingChild(Edition local, Edition remote, Book entity)
        {
            local.BookId = entity.Id;
            local.Book = entity;

            remote.UseDbFieldsFrom(local);
        }

        protected override void AddChildren(List<Edition> children)
        {
            // hack - add the chilren in refresh children so we can control monitored status
        }

        private void MonitorSingleEdition(List<Edition> releases)
        {
            var monitored = releases.Where(x => x.Monitored).ToList();
            if (!monitored.Any())
            {
                monitored = releases;
            }

            var toMonitor = monitored.OrderByDescending(x => _mediaFileService.GetFilesByEdition(x.Id).Count)
                .ThenByDescending(x => x.Ratings.Votes)
                .First();

            releases.ForEach(x => x.Monitored = false);
            toMonitor.Monitored = true;

            Debug.Assert(!releases.Any() || releases.Count(x => x.Monitored) == 1, "one edition monitored");
        }

        protected override bool RefreshChildren(SortedChildren localChildren, List<Edition> remoteChildren, Author remoteData, bool forceChildRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            // make sure only one of the releases ends up monitored
            localChildren.Old.ForEach(x => x.Monitored = false);
            MonitorSingleEdition(localChildren.Future);

            localChildren.All.ForEach(x => _logger.Trace($"release: {x} monitored: {x.Monitored}"));

            _editionService.InsertMany(localChildren.Added);

            return _refreshEditionService.RefreshEditionInfo(localChildren.Added, localChildren.Updated, localChildren.Merged, localChildren.Deleted, localChildren.UpToDate, remoteChildren, forceUpdateFileTags);
        }

        protected override void PublishEntityUpdatedEvent(Book entity)
        {
            // Fetch fresh from DB so all lazy loads are available
            _eventAggregator.PublishEvent(new BookUpdatedEvent(_bookService.GetBook(entity.Id)));
        }

        public bool RefreshBookInfo(List<Book> books, List<Book> remoteBooks, Author remoteData, bool forceAlbumRefresh, bool forceUpdateFileTags, DateTime? lastUpdate)
        {
            var updated = false;

            HashSet<string> updatedGoodreadsBooks = null;

            if (lastUpdate.HasValue && lastUpdate.Value.AddDays(14) > DateTime.UtcNow)
            {
                updatedGoodreadsBooks = _bookInfo.GetChangedBooks(lastUpdate.Value);
            }

            foreach (var book in books)
            {
                if (forceAlbumRefresh ||
                    (updatedGoodreadsBooks == null && _checkIfBookShouldBeRefreshed.ShouldRefresh(book)) ||
                    (updatedGoodreadsBooks != null && updatedGoodreadsBooks.Contains(book.ForeignBookId)))
                {
                    updated |= RefreshBookInfo(book, remoteBooks, remoteData, forceUpdateFileTags);
                }
                else
                {
                    _logger.Debug("Skipping refresh of book: {0}", book.Title);
                }
            }

            return updated;
        }

        public bool RefreshBookInfo(Book book, List<Book> remoteBooks, Author remoteData, bool forceUpdateFileTags)
        {
            return RefreshEntityInfo(book, remoteBooks, remoteData, true, forceUpdateFileTags, null);
        }
    }
}
