using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Commands;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Extras;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles.BookImport
{
    public interface IImportApprovedBooks
    {
        List<ImportResult> Import(List<ImportDecision<LocalBook>> decisions, bool replaceExisting, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto);
    }

    public class ImportApprovedBooks : IImportApprovedBooks
    {
        private readonly IUpgradeMediaFiles _bookFileUpgrader;
        private readonly IMediaFileService _mediaFileService;
        private readonly IAudioTagService _audioTagService;
        private readonly IAuthorService _authorService;
        private readonly IAddAuthorService _addAuthorService;
        private readonly IBookService _bookService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IRecycleBinProvider _recycleBinProvider;
        private readonly IExtraService _extraService;
        private readonly IDiskProvider _diskProvider;
        private readonly IEventAggregator _eventAggregator;
        private readonly IManageCommandQueue _commandQueueManager;
        private readonly Logger _logger;

        public ImportApprovedBooks(IUpgradeMediaFiles trackFileUpgrader,
                                    IMediaFileService mediaFileService,
                                    IAudioTagService audioTagService,
                                    IAuthorService authorService,
                                    IAddAuthorService addAuthorService,
                                    IBookService bookService,
                                    IRootFolderService rootFolderService,
                                    IRecycleBinProvider recycleBinProvider,
                                    IExtraService extraService,
                                    IDiskProvider diskProvider,
                                    IEventAggregator eventAggregator,
                                    IManageCommandQueue commandQueueManager,
                                    Logger logger)
        {
            _bookFileUpgrader = trackFileUpgrader;
            _mediaFileService = mediaFileService;
            _audioTagService = audioTagService;
            _authorService = authorService;
            _addAuthorService = addAuthorService;
            _bookService = bookService;
            _rootFolderService = rootFolderService;
            _recycleBinProvider = recycleBinProvider;
            _extraService = extraService;
            _diskProvider = diskProvider;
            _eventAggregator = eventAggregator;
            _commandQueueManager = commandQueueManager;
            _logger = logger;
        }

        public List<ImportResult> Import(List<ImportDecision<LocalBook>> decisions, bool replaceExisting, DownloadClientItem downloadClientItem = null, ImportMode importMode = ImportMode.Auto)
        {
            var importResults = new List<ImportResult>();
            var allImportedTrackFiles = new List<BookFile>();
            var allOldTrackFiles = new List<BookFile>();
            var addedAuthors = new List<Author>();

            var bookDecisions = decisions.Where(e => e.Item.Book != null && e.Approved)
                .GroupBy(e => e.Item.Book.ForeignBookId).ToList();

            int iDecision = 1;
            foreach (var albumDecision in bookDecisions)
            {
                _logger.ProgressInfo($"Importing book {iDecision++}/{bookDecisions.Count} {albumDecision.First().Item.Book}");

                var decisionList = albumDecision.ToList();

                var author = EnsureAuthorAdded(decisionList, addedAuthors);

                if (author == null)
                {
                    // failed to add the author, carry on with next book
                    continue;
                }

                var book = EnsureBookAdded(decisionList);

                if (book == null)
                {
                    // failed to add the book, carry on with next one
                    continue;
                }

                if (replaceExisting)
                {
                    RemoveExistingTrackFiles(author, book);
                }

                // Publish book edited event.
                // Deliberatly don't put in the old book since we don't want to trigger an ArtistScan.
                _eventAggregator.PublishEvent(new BookEditedEvent(book, book));
            }

            var qualifiedImports = decisions.Where(c => c.Approved)
                .GroupBy(c => c.Item.Author.Id, (i, s) => s
                         .OrderByDescending(c => c.Item.Quality, new QualityModelComparer(s.First().Item.Author.QualityProfile))
                         .ThenByDescending(c => c.Item.Size))
                .SelectMany(c => c)
                .ToList();

            _logger.ProgressInfo($"Importing {qualifiedImports.Count} files");
            _logger.Debug($"Importing {qualifiedImports.Count} files. replaceExisting: {replaceExisting}");

            var filesToAdd = new List<BookFile>(qualifiedImports.Count);
            var trackImportedEvents = new List<TrackImportedEvent>(qualifiedImports.Count);

            foreach (var importDecision in qualifiedImports.OrderByDescending(e => e.Item.Size))
            {
                var localTrack = importDecision.Item;
                var oldFiles = new List<BookFile>();

                try
                {
                    //check if already imported
                    if (importResults.Select(r => r.ImportDecision.Item.Book.Id).Contains(localTrack.Book.Id))
                    {
                        importResults.Add(new ImportResult(importDecision, "Book has already been imported"));
                        continue;
                    }

                    localTrack.Book.Author = localTrack.Author;

                    var bookFile = new BookFile
                    {
                        Path = localTrack.Path.CleanFilePath(),
                        Size = localTrack.Size,
                        Modified = localTrack.Modified,
                        DateAdded = DateTime.UtcNow,
                        ReleaseGroup = localTrack.ReleaseGroup,
                        Quality = localTrack.Quality,
                        MediaInfo = localTrack.FileTrackInfo.MediaInfo,
                        BookId = localTrack.Book.Id,
                        Author = localTrack.Author,
                        Book = localTrack.Book
                    };

                    bool copyOnly;
                    switch (importMode)
                    {
                        default:
                        case ImportMode.Auto:
                            copyOnly = downloadClientItem != null && !downloadClientItem.CanMoveFiles;
                            break;
                        case ImportMode.Move:
                            copyOnly = false;
                            break;
                        case ImportMode.Copy:
                            copyOnly = true;
                            break;
                    }

                    if (!localTrack.ExistingFile)
                    {
                        bookFile.SceneName = GetSceneReleaseName(downloadClientItem);

                        var moveResult = _bookFileUpgrader.UpgradeBookFile(bookFile, localTrack, copyOnly);
                        oldFiles = moveResult.OldFiles;
                    }
                    else
                    {
                        // Delete existing files from the DB mapped to this path
                        var previousFile = _mediaFileService.GetFileWithPath(bookFile.Path);

                        if (previousFile != null)
                        {
                            _mediaFileService.Delete(previousFile, DeleteMediaFileReason.ManualOverride);
                        }

                        var rootFolder = _rootFolderService.GetBestRootFolder(localTrack.Path);
                        if (rootFolder.IsCalibreLibrary)
                        {
                            bookFile.CalibreId = bookFile.Path.ParseCalibreId();
                        }

                        _audioTagService.WriteTags(bookFile, false);
                    }

                    filesToAdd.Add(bookFile);
                    importResults.Add(new ImportResult(importDecision));

                    if (!localTrack.ExistingFile)
                    {
                        _extraService.ImportTrack(localTrack, bookFile, copyOnly);
                    }

                    allImportedTrackFiles.Add(bookFile);
                    allOldTrackFiles.AddRange(oldFiles);

                    // create all the import events here, but we can't publish until the trackfiles have been
                    // inserted and ids created
                    trackImportedEvents.Add(new TrackImportedEvent(localTrack, bookFile, oldFiles, !localTrack.ExistingFile, downloadClientItem));
                }
                catch (RootFolderNotFoundException e)
                {
                    _logger.Warn(e, "Couldn't import book " + localTrack);
                    _eventAggregator.PublishEvent(new TrackImportFailedEvent(e, localTrack, !localTrack.ExistingFile, downloadClientItem));

                    importResults.Add(new ImportResult(importDecision, "Failed to import book, Root folder missing."));
                }
                catch (DestinationAlreadyExistsException e)
                {
                    _logger.Warn(e, "Couldn't import book " + localTrack);
                    importResults.Add(new ImportResult(importDecision, "Failed to import book, Destination already exists."));
                }
                catch (UnauthorizedAccessException e)
                {
                    _logger.Warn(e, "Couldn't import book " + localTrack);
                    _eventAggregator.PublishEvent(new TrackImportFailedEvent(e, localTrack, !localTrack.ExistingFile, downloadClientItem));

                    importResults.Add(new ImportResult(importDecision, "Failed to import book, Permissions error"));
                }
                catch (Exception e)
                {
                    _logger.Warn(e, "Couldn't import book " + localTrack);
                    importResults.Add(new ImportResult(importDecision, "Failed to import book"));
                }
            }

            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            _mediaFileService.AddMany(filesToAdd);
            _logger.Debug($"Inserted new trackfiles in {watch.ElapsedMilliseconds}ms");

            // now that trackfiles have been inserted and ids generated, publish the import events
            foreach (var trackImportedEvent in trackImportedEvents)
            {
                _eventAggregator.PublishEvent(trackImportedEvent);
            }

            var albumImports = importResults.Where(e => e.ImportDecision.Item.Book != null)
                .GroupBy(e => e.ImportDecision.Item.Book.Id).ToList();

            foreach (var albumImport in albumImports)
            {
                var book = albumImport.First().ImportDecision.Item.Book;
                var author = albumImport.First().ImportDecision.Item.Author;

                if (albumImport.Where(e => e.Errors.Count == 0).ToList().Count > 0 && author != null && book != null)
                {
                    _eventAggregator.PublishEvent(new BookImportedEvent(
                        author,
                        book,
                        allImportedTrackFiles.Where(s => s.BookId == book.Id).ToList(),
                        allOldTrackFiles.Where(s => s.BookId == book.Id).ToList(),
                        replaceExisting,
                        downloadClientItem));
                }
            }

            //Adding all the rejected decisions
            importResults.AddRange(decisions.Where(c => !c.Approved)
                                            .Select(d => new ImportResult(d, d.Rejections.Select(r => r.Reason).ToArray())));

            // Refresh any artists we added
            if (addedAuthors.Any())
            {
                _commandQueueManager.Push(new BulkRefreshAuthorCommand(addedAuthors.Select(x => x.Id).ToList(), true));
            }

            return importResults;
        }

        private Author EnsureAuthorAdded(List<ImportDecision<LocalBook>> decisions, List<Author> addedArtists)
        {
            var author = decisions.First().Item.Author;

            if (author.Id == 0)
            {
                var dbArtist = _authorService.FindById(author.ForeignAuthorId);

                if (dbArtist == null)
                {
                    _logger.Debug($"Adding remote author {author}");
                    var path = decisions.First().Item.Path;
                    var rootFolder = _rootFolderService.GetBestRootFolder(path);

                    author.RootFolderPath = rootFolder.Path;
                    author.MetadataProfileId = rootFolder.DefaultMetadataProfileId;
                    author.QualityProfileId = rootFolder.DefaultQualityProfileId;
                    author.Monitored = rootFolder.DefaultMonitorOption != MonitorTypes.None;
                    author.Tags = rootFolder.DefaultTags;
                    author.AddOptions = new AddAuthorOptions
                    {
                        SearchForMissingAlbums = false,
                        Monitored = author.Monitored,
                        Monitor = rootFolder.DefaultMonitorOption
                    };

                    if (rootFolder.IsCalibreLibrary)
                    {
                        // calibre has author / book / files
                        author.Path = path.GetParentPath().GetParentPath();
                    }

                    try
                    {
                        dbArtist = _addAuthorService.AddAuthor(author, false);
                        addedArtists.Add(dbArtist);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Failed to add author {0}", author);
                        foreach (var decision in decisions)
                        {
                            decision.Reject(new Rejection("Failed to add missing author", RejectionType.Temporary));
                        }

                        return null;
                    }
                }

                // Put in the newly loaded author
                foreach (var decision in decisions)
                {
                    decision.Item.Author = dbArtist;
                    decision.Item.Book.Author = dbArtist;
                    decision.Item.Book.AuthorMetadataId = dbArtist.AuthorMetadataId;
                }

                author = dbArtist;
            }

            return author;
        }

        private Book EnsureBookAdded(List<ImportDecision<LocalBook>> decisions)
        {
            var book = decisions.First().Item.Book;

            if (book.Id == 0)
            {
                var dbAlbum = _bookService.FindById(book.ForeignBookId);

                if (dbAlbum == null)
                {
                    _logger.Debug($"Adding remote book {book}");
                    try
                    {
                        book.Added = DateTime.UtcNow;
                        _bookService.InsertMany(new List<Book> { book });
                        dbAlbum = _bookService.FindById(book.ForeignBookId);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Failed to add book {0}", book);
                        RejectAlbum(decisions);

                        return null;
                    }
                }

                // Populate the new DB book
                foreach (var decision in decisions)
                {
                    decision.Item.Book = dbAlbum;
                }
            }

            return book;
        }

        private void RejectAlbum(List<ImportDecision<LocalBook>> decisions)
        {
            foreach (var decision in decisions)
            {
                decision.Reject(new Rejection("Failed to add missing book", RejectionType.Temporary));
            }
        }

        private void RemoveExistingTrackFiles(Author author, Book book)
        {
            var rootFolder = _diskProvider.GetParentFolder(author.Path);
            var previousFiles = _mediaFileService.GetFilesByBook(book.Id);

            _logger.Debug($"Deleting {previousFiles.Count} existing files for {book}");

            foreach (var previousFile in previousFiles)
            {
                var subfolder = rootFolder.GetRelativePath(_diskProvider.GetParentFolder(previousFile.Path));
                if (_diskProvider.FileExists(previousFile.Path))
                {
                    _logger.Debug("Removing existing book file: {0}", previousFile);
                    _recycleBinProvider.DeleteFile(previousFile.Path, subfolder);
                }

                _mediaFileService.Delete(previousFile, DeleteMediaFileReason.Upgrade);
            }
        }

        private string GetSceneReleaseName(DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem != null)
            {
                var title = Parser.Parser.RemoveFileExtension(downloadClientItem.Title);

                var parsedTitle = Parser.Parser.ParseBookTitle(title);

                if (parsedTitle != null)
                {
                    return title;
                }
            }

            return null;
        }
    }
}
