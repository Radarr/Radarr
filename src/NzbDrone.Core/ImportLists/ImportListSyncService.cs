using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.ImportLists.Exclusions;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.MetadataSource;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.ImportLists
{
    public class ImportListSyncService : IExecute<ImportListSyncCommand>
    {
        private readonly IImportListFactory _importListFactory;
        private readonly IImportListExclusionService _importListExclusionService;
        private readonly IFetchAndParseImportList _listFetcherAndParser;
        private readonly ISearchForNewBook _bookSearchService;
        private readonly ISearchForNewAuthor _authorSearchService;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IAddAuthorService _addAuthorService;
        private readonly IAddBookService _addBookService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public ImportListSyncService(IImportListFactory importListFactory,
                                     IImportListExclusionService importListExclusionService,
                                     IFetchAndParseImportList listFetcherAndParser,
                                     ISearchForNewBook bookSearchService,
                                     ISearchForNewAuthor authorSearchService,
                                     IAuthorService authorService,
                                     IBookService bookService,
                                     IAddAuthorService addAuthorService,
                                     IAddBookService addBookService,
                                     IEventAggregator eventAggregator,
                                     IManageCommandQueue commandQueueManager,
                                     Logger logger)
        {
            _importListFactory = importListFactory;
            _importListExclusionService = importListExclusionService;
            _listFetcherAndParser = listFetcherAndParser;
            _bookSearchService = bookSearchService;
            _authorSearchService = authorSearchService;
            _authorService = authorService;
            _bookService = bookService;
            _addAuthorService = addAuthorService;
            _addBookService = addBookService;
            _eventAggregator = eventAggregator;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        private List<Book> SyncAll()
        {
            _logger.ProgressInfo("Starting Import List Sync");

            var rssReleases = _listFetcherAndParser.Fetch();

            var reports = rssReleases.ToList();

            return ProcessReports(reports);
        }

        private List<Book> SyncList(ImportListDefinition definition)
        {
            _logger.ProgressInfo(string.Format("Starting Import List Refresh for List {0}", definition.Name));

            var rssReleases = _listFetcherAndParser.FetchSingleList(definition);

            var reports = rssReleases.ToList();

            return ProcessReports(reports);
        }

        private List<Book> ProcessReports(List<ImportListItemInfo> reports)
        {
            var processed = new List<Book>();
            var authorsToAdd = new List<Author>();
            var booksToAdd = new List<Book>();

            _logger.ProgressInfo("Processing {0} list items", reports.Count);

            var reportNumber = 1;

            var listExclusions = _importListExclusionService.All();

            foreach (var report in reports)
            {
                _logger.ProgressTrace("Processing list item {0}/{1}", reportNumber, reports.Count);

                reportNumber++;

                var importList = _importListFactory.Get(report.ImportListId);

                if (report.Book.IsNotNullOrWhiteSpace() || report.EditionGoodreadsId.IsNotNullOrWhiteSpace())
                {
                    if (report.EditionGoodreadsId.IsNullOrWhiteSpace() || report.AuthorGoodreadsId.IsNullOrWhiteSpace())
                    {
                        MapBookReport(report);
                    }

                    ProcessBookReport(importList, report, listExclusions, booksToAdd);
                }
                else if (report.Author.IsNotNullOrWhiteSpace() || report.AuthorGoodreadsId.IsNotNullOrWhiteSpace())
                {
                    if (report.AuthorGoodreadsId.IsNullOrWhiteSpace())
                    {
                        MapAuthorReport(report);
                    }

                    ProcessAuthorReport(importList, report, listExclusions, authorsToAdd);
                }
            }

            var addedAuthors = _addAuthorService.AddAuthors(authorsToAdd, false);
            var addedBooks = _addBookService.AddBooks(booksToAdd, false);

            var message = string.Format($"Import List Sync Completed. Items found: {reports.Count}, Authors added: {authorsToAdd.Count}, Books added: {booksToAdd.Count}");

            _logger.ProgressInfo(message);

            var toRefresh = addedAuthors.Select(x => x.Id).Concat(addedBooks.Select(x => x.Author.Value.Id)).Distinct().ToList();
            if (toRefresh.Any())
            {
                _commandQueueManager.Push(new BulkRefreshAuthorCommand(toRefresh, true));
            }

            return processed;
        }

        private void MapBookReport(ImportListItemInfo report)
        {
            Book mappedBook;

            if (report.EditionGoodreadsId.IsNotNullOrWhiteSpace() && int.TryParse(report.EditionGoodreadsId, out var goodreadsId))
            {
                var search = _bookSearchService.SearchByGoodreadsId(goodreadsId);
                mappedBook = search.FirstOrDefault(x => x.Editions.Value.Any(e => int.TryParse(e.ForeignEditionId, out var editionId) && editionId == goodreadsId));
            }
            else
            {
                mappedBook = _bookSearchService.SearchForNewBook(report.Book, report.Author).FirstOrDefault();
            }

            // Break if we are looking for an book and cant find it. This will avoid us from adding the author and possibly getting it wrong.
            if (mappedBook == null)
            {
                _logger.Trace($"Nothing found for {report.EditionGoodreadsId}");
                report.EditionGoodreadsId = null;
                return;
            }

            _logger.Trace($"Mapped {report.EditionGoodreadsId} to {mappedBook}");

            report.BookGoodreadsId = mappedBook.ForeignBookId;
            report.Book = mappedBook.Title;
            report.Author = mappedBook.AuthorMetadata?.Value?.Name;
            report.AuthorGoodreadsId = mappedBook.AuthorMetadata?.Value?.ForeignAuthorId;
        }

        private void ProcessBookReport(ImportListDefinition importList, ImportListItemInfo report, List<ImportListExclusion> listExclusions, List<Book> booksToAdd)
        {
            if (report.EditionGoodreadsId == null)
            {
                return;
            }

            // Check to see if book in DB
            var existingBook = _bookService.FindById(report.BookGoodreadsId);

            // Check to see if book excluded
            var excludedBook = listExclusions.SingleOrDefault(s => s.ForeignId == report.BookGoodreadsId);

            // Check to see if author excluded
            var excludedAuthor = listExclusions.SingleOrDefault(s => s.ForeignId == report.AuthorGoodreadsId);

            if (excludedBook != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.EditionGoodreadsId, report.Book);
                return;
            }

            if (excludedAuthor != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion for parent author", report.EditionGoodreadsId, report.Book);
                return;
            }

            if (existingBook != null)
            {
                _logger.Debug("{0} [{1}] Rejected, Book Exists in DB.  Ensuring Book and Author monitored.", report.EditionGoodreadsId, report.Book);

                if (importList.ShouldMonitor != ImportListMonitorType.None)
                {
                    if (!existingBook.Monitored)
                    {
                        _bookService.SetBookMonitored(existingBook.Id, true);
                    }

                    var existingAuthor = existingBook.Author.Value;
                    if (importList.ShouldMonitor == ImportListMonitorType.EntireAuthor)
                    {
                        _bookService.SetMonitored(existingAuthor.Books.Value.Select(x => x.Id), true);
                    }

                    if (!existingAuthor.Monitored)
                    {
                        existingAuthor.Monitored = true;
                        _authorService.UpdateAuthor(existingAuthor);
                    }
                }

                return;
            }

            // Append Book if not already in DB or already on add list
            if (booksToAdd.All(s => s.ForeignBookId != report.BookGoodreadsId))
            {
                var monitored = importList.ShouldMonitor != ImportListMonitorType.None;

                var toAdd = new Book
                {
                    ForeignBookId = report.BookGoodreadsId,
                    Monitored = monitored,
                    Editions = new List<Edition>
                    {
                        new Edition
                        {
                            ForeignEditionId = report.EditionGoodreadsId,
                            Monitored = true
                        }
                    },
                    Author = new Author
                    {
                        Monitored = monitored,
                        RootFolderPath = importList.RootFolderPath,
                        QualityProfileId = importList.ProfileId,
                        MetadataProfileId = importList.MetadataProfileId,
                        Tags = importList.Tags,
                        AddOptions = new AddAuthorOptions
                        {
                            SearchForMissingBooks = importList.ShouldSearch,
                            Monitored = monitored,
                            Monitor = monitored ? MonitorTypes.All : MonitorTypes.None
                        }
                    },
                    AddOptions = new AddBookOptions
                    {
                        SearchForNewBook = monitored
                    }
                };

                if (importList.ShouldMonitor == ImportListMonitorType.SpecificBook)
                {
                    toAdd.Author.Value.AddOptions.BooksToMonitor.Add(toAdd.ForeignBookId);
                }

                booksToAdd.Add(toAdd);
            }
        }

        private void MapAuthorReport(ImportListItemInfo report)
        {
            var mappedAuthor = _authorSearchService.SearchForNewAuthor(report.Author)
                .FirstOrDefault();
            report.AuthorGoodreadsId = mappedAuthor?.Metadata.Value?.ForeignAuthorId;
            report.Author = mappedAuthor?.Metadata.Value?.Name;
        }

        private void ProcessAuthorReport(ImportListDefinition importList, ImportListItemInfo report, List<ImportListExclusion> listExclusions, List<Author> authorsToAdd)
        {
            if (report.AuthorGoodreadsId == null)
            {
                return;
            }

            // Check to see if author in DB
            var existingAuthor = _authorService.FindById(report.AuthorGoodreadsId);

            // Check to see if author excluded
            var excludedAuthor = listExclusions.Where(s => s.ForeignId == report.AuthorGoodreadsId).SingleOrDefault();

            if (excludedAuthor != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.AuthorGoodreadsId, report.Author);
                return;
            }

            if (existingAuthor != null)
            {
                _logger.Debug("{0} [{1}] Rejected, Author Exists in DB.  Ensuring Author monitored", report.AuthorGoodreadsId, report.Author);

                if (!existingAuthor.Monitored)
                {
                    existingAuthor.Monitored = true;
                    _authorService.UpdateAuthor(existingAuthor);
                }

                return;
            }

            // Append Author if not already in DB or already on add list
            if (authorsToAdd.All(s => s.Metadata.Value.ForeignAuthorId != report.AuthorGoodreadsId))
            {
                var monitored = importList.ShouldMonitor != ImportListMonitorType.None;

                authorsToAdd.Add(new Author
                {
                    Metadata = new AuthorMetadata
                    {
                        ForeignAuthorId = report.AuthorGoodreadsId,
                        Name = report.Author
                    },
                    Monitored = monitored,
                    RootFolderPath = importList.RootFolderPath,
                    QualityProfileId = importList.ProfileId,
                    MetadataProfileId = importList.MetadataProfileId,
                    Tags = importList.Tags,
                    AddOptions = new AddAuthorOptions
                    {
                        SearchForMissingBooks = importList.ShouldSearch,
                        Monitored = monitored,
                        Monitor = monitored ? MonitorTypes.All : MonitorTypes.None
                    }
                });
            }
        }

        public void Execute(ImportListSyncCommand message)
        {
            List<Book> processed;

            if (message.DefinitionId.HasValue)
            {
                processed = SyncList(_importListFactory.Get(message.DefinitionId.Value));
            }
            else
            {
                processed = SyncAll();
            }

            _eventAggregator.PublishEvent(new ImportListSyncCompleteEvent(processed));
        }
    }
}
