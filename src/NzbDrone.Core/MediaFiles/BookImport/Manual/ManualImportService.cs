using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.Crypto;
using NzbDrone.Common.Disk;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Instrumentation.Extensions;
using NzbDrone.Core.Books;
using NzbDrone.Core.DecisionEngine;
using NzbDrone.Core.Download;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Commands;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.RootFolders;

namespace NzbDrone.Core.MediaFiles.BookImport.Manual
{
    public interface IManualImportService
    {
        List<ManualImportItem> GetMediaFiles(string path, string downloadId, FilterFilesType filter, bool replaceExistingFiles);
        List<ManualImportItem> UpdateItems(List<ManualImportItem> item);
    }

    public class ManualImportService : IExecute<ManualImportCommand>, IManualImportService
    {
        private readonly IDiskProvider _diskProvider;
        private readonly IParsingService _parsingService;
        private readonly IRootFolderService _rootFolderService;
        private readonly IDiskScanService _diskScanService;
        private readonly IMakeImportDecision _importDecisionMaker;
        private readonly IAuthorService _authorService;
        private readonly IBookService _bookService;
        private readonly IAudioTagService _audioTagService;
        private readonly IImportApprovedBooks _importApprovedBooks;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IDownloadedBooksImportService _downloadedTracksImportService;
        private readonly IEventAggregator _eventAggregator;
        private readonly Logger _logger;

        public ManualImportService(IDiskProvider diskProvider,
                                   IParsingService parsingService,
                                   IRootFolderService rootFolderService,
                                   IDiskScanService diskScanService,
                                   IMakeImportDecision importDecisionMaker,
                                   IAuthorService authorService,
                                   IBookService bookService,
                                   IAudioTagService audioTagService,
                                   IImportApprovedBooks importApprovedBooks,
                                   ITrackedDownloadService trackedDownloadService,
                                   IDownloadedBooksImportService downloadedTracksImportService,
                                   IEventAggregator eventAggregator,
                                   Logger logger)
        {
            _diskProvider = diskProvider;
            _parsingService = parsingService;
            _rootFolderService = rootFolderService;
            _diskScanService = diskScanService;
            _importDecisionMaker = importDecisionMaker;
            _authorService = authorService;
            _bookService = bookService;
            _audioTagService = audioTagService;
            _importApprovedBooks = importApprovedBooks;
            _trackedDownloadService = trackedDownloadService;
            _downloadedTracksImportService = downloadedTracksImportService;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public List<ManualImportItem> GetMediaFiles(string path, string downloadId, FilterFilesType filter, bool replaceExistingFiles)
        {
            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);

                if (trackedDownload == null)
                {
                    return new List<ManualImportItem>();
                }

                path = trackedDownload.DownloadItem.OutputPath.FullPath;
            }

            if (!_diskProvider.FolderExists(path))
            {
                if (!_diskProvider.FileExists(path))
                {
                    return new List<ManualImportItem>();
                }

                var files = new List<IFileInfo> { _diskProvider.GetFileInfo(path) };

                var config = new ImportDecisionMakerConfig
                {
                    Filter = FilterFilesType.None,
                    NewDownload = true,
                    SingleRelease = false,
                    IncludeExisting = !replaceExistingFiles,
                    AddNewAuthors = false
                };

                var decision = _importDecisionMaker.GetImportDecisions(files, null, null, config);
                var result = MapItem(decision.First(), downloadId, replaceExistingFiles);

                return new List<ManualImportItem> { result };
            }

            return ProcessFolder(path, downloadId, filter, replaceExistingFiles);
        }

        private List<ManualImportItem> ProcessFolder(string folder, string downloadId, FilterFilesType filter, bool replaceExistingFiles)
        {
            DownloadClientItem downloadClientItem = null;
            var directoryInfo = new DirectoryInfo(folder);
            var author = _parsingService.GetArtist(directoryInfo.Name);

            if (downloadId.IsNotNullOrWhiteSpace())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);
                downloadClientItem = trackedDownload.DownloadItem;

                if (author == null)
                {
                    author = trackedDownload.RemoteBook?.Author;
                }
            }

            var authorFiles = _diskScanService.GetBookFiles(folder).ToList();
            var idOverrides = new IdentificationOverrides
            {
                Author = author
            };
            var itemInfo = new ImportDecisionMakerInfo
            {
                DownloadClientItem = downloadClientItem,
                ParsedTrackInfo = Parser.Parser.ParseTitle(directoryInfo.Name)
            };
            var config = new ImportDecisionMakerConfig
            {
                Filter = filter,
                NewDownload = true,
                SingleRelease = false,
                IncludeExisting = !replaceExistingFiles,
                AddNewAuthors = false
            };

            var decisions = _importDecisionMaker.GetImportDecisions(authorFiles, idOverrides, itemInfo, config);

            // paths will be different for new and old files which is why we need to map separately
            var newFiles = authorFiles.Join(decisions,
                                            f => f.FullName,
                                            d => d.Item.Path,
                                            (f, d) => new { File = f, Decision = d },
                                            PathEqualityComparer.Instance);

            var newItems = newFiles.Select(x => MapItem(x.Decision, downloadId, replaceExistingFiles));
            var existingDecisions = decisions.Except(newFiles.Select(x => x.Decision));
            var existingItems = existingDecisions.Select(x => MapItem(x, null, replaceExistingFiles));

            return newItems.Concat(existingItems).ToList();
        }

        public List<ManualImportItem> UpdateItems(List<ManualImportItem> items)
        {
            var replaceExistingFiles = items.All(x => x.ReplaceExistingFiles);
            var groupedItems = items.Where(x => !x.AdditionalFile).GroupBy(x => x.Book?.Id);
            _logger.Debug($"UpdateItems, {groupedItems.Count()} groups, replaceExisting {replaceExistingFiles}");

            var result = new List<ManualImportItem>();

            foreach (var group in groupedItems)
            {
                _logger.Debug("UpdateItems, group key: {0}", group.Key);

                var files = group.Select(x => _diskProvider.GetFileInfo(x.Path)).ToList();
                var idOverride = new IdentificationOverrides
                {
                    Author = group.First().Author,
                    Album = group.First().Book,
                };
                var config = new ImportDecisionMakerConfig
                {
                    Filter = FilterFilesType.None,
                    NewDownload = true,
                    SingleRelease = true,
                    IncludeExisting = !replaceExistingFiles,
                    AddNewAuthors = false
                };
                var decisions = _importDecisionMaker.GetImportDecisions(files, idOverride, null, config);

                var existingItems = group.Join(decisions,
                                               i => i.Path,
                                               d => d.Item.Path,
                                               (i, d) => new { Item = i, Decision = d },
                                               PathEqualityComparer.Instance);

                foreach (var pair in existingItems)
                {
                    var item = pair.Item;
                    var decision = pair.Decision;

                    if (decision.Item.Author != null)
                    {
                        item.Author = decision.Item.Author;
                    }

                    if (decision.Item.Book != null)
                    {
                        item.Book = decision.Item.Book;
                    }

                    item.Rejections = decision.Rejections;

                    result.Add(item);
                }

                var newDecisions = decisions.Except(existingItems.Select(x => x.Decision));
                result.AddRange(newDecisions.Select(x => MapItem(x, null, replaceExistingFiles)));
            }

            return result;
        }

        private ManualImportItem MapItem(ImportDecision<LocalBook> decision, string downloadId, bool replaceExistingFiles)
        {
            var item = new ManualImportItem();

            item.Id = HashConverter.GetHashInt31(decision.Item.Path);
            item.Path = decision.Item.Path;
            item.Name = Path.GetFileNameWithoutExtension(decision.Item.Path);
            item.DownloadId = downloadId;

            if (decision.Item.Author != null)
            {
                item.Author = decision.Item.Author;
            }

            if (decision.Item.Book != null)
            {
                item.Book = decision.Item.Book;
            }

            item.Quality = decision.Item.Quality;
            item.Size = _diskProvider.GetFileSize(decision.Item.Path);
            item.Rejections = decision.Rejections;
            item.Tags = decision.Item.FileTrackInfo;
            item.AdditionalFile = decision.Item.AdditionalFile;
            item.ReplaceExistingFiles = replaceExistingFiles;

            return item;
        }

        public void Execute(ManualImportCommand message)
        {
            _logger.ProgressTrace("Manually importing {0} files using mode {1}", message.Files.Count, message.ImportMode);

            var imported = new List<ImportResult>();
            var importedTrackedDownload = new List<ManuallyImportedFile>();
            var bookIds = message.Files.GroupBy(e => e.BookId).ToList();
            var fileCount = 0;

            foreach (var importBookId in bookIds)
            {
                var albumImportDecisions = new List<ImportDecision<LocalBook>>();

                foreach (var file in importBookId)
                {
                    _logger.ProgressTrace("Processing file {0} of {1}", fileCount + 1, message.Files.Count);

                    var author = _authorService.GetAuthor(file.AuthorId);
                    var book = _bookService.GetBook(file.BookId);
                    var fileTrackInfo = _audioTagService.ReadTags(file.Path) ?? new ParsedTrackInfo();
                    var fileInfo = _diskProvider.GetFileInfo(file.Path);

                    var localTrack = new LocalBook
                    {
                        ExistingFile = author.Path.IsParentPath(file.Path),
                        FileTrackInfo = fileTrackInfo,
                        Path = file.Path,
                        Size = fileInfo.Length,
                        Modified = fileInfo.LastWriteTimeUtc,
                        Quality = file.Quality,
                        Author = author,
                        Book = book
                    };

                    var importDecision = new ImportDecision<LocalBook>(localTrack);
                    if (_rootFolderService.GetBestRootFolder(author.Path) == null)
                    {
                        _logger.Warn($"Destination author folder {author.Path} not in a Root Folder, skipping import");
                        importDecision.Reject(new Rejection($"Destination author folder {author.Path} is not in a Root Folder"));
                    }

                    albumImportDecisions.Add(importDecision);
                    fileCount += 1;
                }

                var downloadId = importBookId.Select(x => x.DownloadId).FirstOrDefault(x => x.IsNotNullOrWhiteSpace());
                if (downloadId.IsNullOrWhiteSpace())
                {
                    imported.AddRange(_importApprovedBooks.Import(albumImportDecisions, message.ReplaceExistingFiles, null, message.ImportMode));
                }
                else
                {
                    var trackedDownload = _trackedDownloadService.Find(downloadId);
                    var importResults = _importApprovedBooks.Import(albumImportDecisions, message.ReplaceExistingFiles, trackedDownload.DownloadItem, message.ImportMode);

                    imported.AddRange(importResults);

                    foreach (var importResult in importResults)
                    {
                        importedTrackedDownload.Add(new ManuallyImportedFile
                        {
                            TrackedDownload = trackedDownload,
                            ImportResult = importResult
                        });
                    }
                }
            }

            _logger.ProgressTrace("Manually imported {0} files", imported.Count);

            foreach (var groupedTrackedDownload in importedTrackedDownload.GroupBy(i => i.TrackedDownload.DownloadItem.DownloadId).ToList())
            {
                var trackedDownload = groupedTrackedDownload.First().TrackedDownload;

                if (_diskProvider.FolderExists(trackedDownload.DownloadItem.OutputPath.FullPath))
                {
                    if (_downloadedTracksImportService.ShouldDeleteFolder(
                            _diskProvider.GetDirectoryInfo(trackedDownload.DownloadItem.OutputPath.FullPath),
                            trackedDownload.RemoteBook.Author) && trackedDownload.DownloadItem.CanMoveFiles)
                    {
                        _diskProvider.DeleteFolder(trackedDownload.DownloadItem.OutputPath.FullPath, true);
                    }
                }

                if (groupedTrackedDownload.Select(c => c.ImportResult).Count(c => c.Result == ImportResultType.Imported) >= Math.Max(1, trackedDownload.RemoteBook.Books.Count))
                {
                    trackedDownload.State = TrackedDownloadState.Imported;
                    _eventAggregator.PublishEvent(new DownloadCompletedEvent(trackedDownload));
                }
            }
        }
    }
}
