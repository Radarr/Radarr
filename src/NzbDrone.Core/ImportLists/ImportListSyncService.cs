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
            var artistsToAdd = new List<Author>();
            var albumsToAdd = new List<Book>();

            _logger.ProgressInfo("Processing {0} list items", reports.Count);

            var reportNumber = 1;

            var listExclusions = _importListExclusionService.All();

            foreach (var report in reports)
            {
                _logger.ProgressTrace("Processing list item {0}/{1}", reportNumber, reports.Count);

                reportNumber++;

                var importList = _importListFactory.Get(report.ImportListId);

                if (report.Book.IsNotNullOrWhiteSpace() || report.AlbumMusicBrainzId.IsNotNullOrWhiteSpace())
                {
                    if (report.AlbumMusicBrainzId.IsNullOrWhiteSpace() || report.ArtistMusicBrainzId.IsNullOrWhiteSpace())
                    {
                        MapAlbumReport(report);
                    }

                    ProcessAlbumReport(importList, report, listExclusions, albumsToAdd);
                }
                else if (report.Author.IsNotNullOrWhiteSpace() || report.ArtistMusicBrainzId.IsNotNullOrWhiteSpace())
                {
                    if (report.ArtistMusicBrainzId.IsNullOrWhiteSpace())
                    {
                        MapArtistReport(report);
                    }

                    ProcessArtistReport(importList, report, listExclusions, artistsToAdd);
                }
            }

            var addedArtists = _addAuthorService.AddAuthors(artistsToAdd, false);
            var addedAlbums = _addBookService.AddBooks(albumsToAdd, false);

            var message = string.Format($"Import List Sync Completed. Items found: {reports.Count}, Authors added: {artistsToAdd.Count}, Books added: {albumsToAdd.Count}");

            _logger.ProgressInfo(message);

            var toRefresh = addedArtists.Select(x => x.Id).Concat(addedAlbums.Select(x => x.Author.Value.Id)).Distinct().ToList();
            if (toRefresh.Any())
            {
                _commandQueueManager.Push(new BulkRefreshAuthorCommand(toRefresh, true));
            }

            return processed;
        }

        private void MapAlbumReport(ImportListItemInfo report)
        {
            Book mappedAlbum;

            if (report.AlbumMusicBrainzId.IsNotNullOrWhiteSpace() && int.TryParse(report.AlbumMusicBrainzId, out var goodreadsId))
            {
                mappedAlbum = _bookSearchService.SearchByGoodreadsId(goodreadsId).FirstOrDefault(x => x.GoodreadsId == goodreadsId);
            }
            else
            {
                mappedAlbum = _bookSearchService.SearchForNewBook(report.Book, report.Author).FirstOrDefault();
            }

            // Break if we are looking for an book and cant find it. This will avoid us from adding the author and possibly getting it wrong.
            if (mappedAlbum == null)
            {
                _logger.Trace($"Nothing found for {report.AlbumMusicBrainzId}");
                report.AlbumMusicBrainzId = null;
                return;
            }

            _logger.Trace($"Mapped {report.AlbumMusicBrainzId} to {mappedAlbum}");

            report.AlbumMusicBrainzId = mappedAlbum.ForeignBookId;
            report.Book = mappedAlbum.Title;
            report.Author = mappedAlbum.AuthorMetadata?.Value?.Name;
            report.ArtistMusicBrainzId = mappedAlbum.AuthorMetadata?.Value?.ForeignAuthorId;
        }

        private void ProcessAlbumReport(ImportListDefinition importList, ImportListItemInfo report, List<ImportListExclusion> listExclusions, List<Book> albumsToAdd)
        {
            if (report.AlbumMusicBrainzId == null)
            {
                return;
            }

            // Check to see if book in DB
            var existingAlbum = _bookService.FindById(report.AlbumMusicBrainzId);

            if (existingAlbum != null)
            {
                _logger.Debug("{0} [{1}] Rejected, Book Exists in DB", report.AlbumMusicBrainzId, report.Book);
                return;
            }

            // Check to see if book excluded
            var excludedAlbum = listExclusions.SingleOrDefault(s => s.ForeignId == report.AlbumMusicBrainzId);

            if (excludedAlbum != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.AlbumMusicBrainzId, report.Book);
                return;
            }

            // Check to see if author excluded
            var excludedArtist = listExclusions.SingleOrDefault(s => s.ForeignId == report.ArtistMusicBrainzId);

            if (excludedArtist != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion for parent author", report.AlbumMusicBrainzId, report.Book);
                return;
            }

            // Append Album if not already in DB or already on add list
            if (albumsToAdd.All(s => s.ForeignBookId != report.AlbumMusicBrainzId))
            {
                var monitored = importList.ShouldMonitor != ImportListMonitorType.None;

                var toAdd = new Book
                {
                    ForeignBookId = report.AlbumMusicBrainzId,
                    Monitored = monitored,
                    Author = new Author
                    {
                        Monitored = monitored,
                        RootFolderPath = importList.RootFolderPath,
                        QualityProfileId = importList.ProfileId,
                        MetadataProfileId = importList.MetadataProfileId,
                        Tags = importList.Tags,
                        AddOptions = new AddAuthorOptions
                        {
                            SearchForMissingBooks = monitored,
                            Monitored = monitored,
                            Monitor = monitored ? MonitorTypes.All : MonitorTypes.None
                        }
                    },
                };

                if (importList.ShouldMonitor == ImportListMonitorType.SpecificAlbum)
                {
                    toAdd.Author.Value.AddOptions.BooksToMonitor.Add(toAdd.ForeignBookId);
                }

                albumsToAdd.Add(toAdd);
            }
        }

        private void MapArtistReport(ImportListItemInfo report)
        {
            var mappedArtist = _authorSearchService.SearchForNewAuthor(report.Author)
                .FirstOrDefault();
            report.ArtistMusicBrainzId = mappedArtist?.Metadata.Value?.ForeignAuthorId;
            report.Author = mappedArtist?.Metadata.Value?.Name;
        }

        private void ProcessArtistReport(ImportListDefinition importList, ImportListItemInfo report, List<ImportListExclusion> listExclusions, List<Author> artistsToAdd)
        {
            if (report.ArtistMusicBrainzId == null)
            {
                return;
            }

            // Check to see if author in DB
            var existingArtist = _authorService.FindById(report.ArtistMusicBrainzId);

            if (existingArtist != null)
            {
                _logger.Debug("{0} [{1}] Rejected, Author Exists in DB", report.ArtistMusicBrainzId, report.Author);
                return;
            }

            // Check to see if author excluded
            var excludedArtist = listExclusions.Where(s => s.ForeignId == report.ArtistMusicBrainzId).SingleOrDefault();

            if (excludedArtist != null)
            {
                _logger.Debug("{0} [{1}] Rejected due to list exlcusion", report.ArtistMusicBrainzId, report.Author);
                return;
            }

            // Append Author if not already in DB or already on add list
            if (artistsToAdd.All(s => s.Metadata.Value.ForeignAuthorId != report.ArtistMusicBrainzId))
            {
                var monitored = importList.ShouldMonitor != ImportListMonitorType.None;

                artistsToAdd.Add(new Author
                {
                    Metadata = new AuthorMetadata
                    {
                        ForeignAuthorId = report.ArtistMusicBrainzId,
                        Name = report.Author
                    },
                    Monitored = monitored,
                    RootFolderPath = importList.RootFolderPath,
                    QualityProfileId = importList.ProfileId,
                    MetadataProfileId = importList.MetadataProfileId,
                    Tags = importList.Tags,
                    AddOptions = new AddAuthorOptions
                    {
                        SearchForMissingBooks = monitored,
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
