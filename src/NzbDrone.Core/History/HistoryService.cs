using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NLog;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Download;
using NzbDrone.Core.MediaFiles;
using NzbDrone.Core.MediaFiles.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.History
{
    public interface IHistoryService
    {
        PagingSpec<History> Paged(PagingSpec<History> pagingSpec);
        History MostRecentForBook(int bookId);
        History MostRecentForDownloadId(string downloadId);
        History Get(int historyId);
        List<History> GetByAuthor(int authorId, HistoryEventType? eventType);
        List<History> GetByBook(int bookId, HistoryEventType? eventType);
        List<History> Find(string downloadId, HistoryEventType eventType);
        List<History> FindByDownloadId(string downloadId);
        List<History> Since(DateTime date, HistoryEventType? eventType);
        void UpdateMany(IList<History> items);
    }

    public class HistoryService : IHistoryService,
                                  IHandle<BookGrabbedEvent>,
                                  IHandle<BookImportIncompleteEvent>,
                                  IHandle<TrackImportedEvent>,
                                  IHandle<DownloadFailedEvent>,
                                  IHandle<DownloadCompletedEvent>,
                                  IHandle<BookFileDeletedEvent>,
                                  IHandle<BookFileRenamedEvent>,
                                  IHandle<BookFileRetaggedEvent>,
                                  IHandle<AuthorDeletedEvent>,
                                  IHandle<DownloadIgnoredEvent>
    {
        private readonly IHistoryRepository _historyRepository;
        private readonly Logger _logger;

        public HistoryService(IHistoryRepository historyRepository, Logger logger)
        {
            _historyRepository = historyRepository;
            _logger = logger;
        }

        public PagingSpec<History> Paged(PagingSpec<History> pagingSpec)
        {
            return _historyRepository.GetPaged(pagingSpec);
        }

        public History MostRecentForBook(int bookId)
        {
            return _historyRepository.MostRecentForBook(bookId);
        }

        public History MostRecentForDownloadId(string downloadId)
        {
            return _historyRepository.MostRecentForDownloadId(downloadId);
        }

        public History Get(int historyId)
        {
            return _historyRepository.Get(historyId);
        }

        public List<History> GetByAuthor(int authorId, HistoryEventType? eventType)
        {
            return _historyRepository.GetByAuthor(authorId, eventType);
        }

        public List<History> GetByBook(int bookId, HistoryEventType? eventType)
        {
            return _historyRepository.GetByBook(bookId, eventType);
        }

        public List<History> Find(string downloadId, HistoryEventType eventType)
        {
            return _historyRepository.FindByDownloadId(downloadId).Where(c => c.EventType == eventType).ToList();
        }

        public List<History> FindByDownloadId(string downloadId)
        {
            return _historyRepository.FindByDownloadId(downloadId);
        }

        private string FindDownloadId(TrackImportedEvent trackedDownload)
        {
            _logger.Debug("Trying to find downloadId for {0} from history", trackedDownload.ImportedBook.Path);

            var bookIds = new List<int> { trackedDownload.BookInfo.Book.Id };

            var allHistory = _historyRepository.FindDownloadHistory(trackedDownload.BookInfo.Author.Id, trackedDownload.ImportedBook.Quality);

            //Find download related items for these episdoes
            var albumsHistory = allHistory.Where(h => bookIds.Contains(h.BookId)).ToList();

            var processedDownloadId = albumsHistory
                .Where(c => c.EventType != HistoryEventType.Grabbed && c.DownloadId != null)
                .Select(c => c.DownloadId);

            var stillDownloading = albumsHistory.Where(c => c.EventType == HistoryEventType.Grabbed && !processedDownloadId.Contains(c.DownloadId)).ToList();

            string downloadId = null;

            if (stillDownloading.Any())
            {
                var matchingHistory = stillDownloading.Where(c => c.BookId == trackedDownload.BookInfo.Book.Id).ToList();

                if (matchingHistory.Count != 1)
                {
                    return null;
                }

                var newDownloadId = matchingHistory.Single().DownloadId;

                if (downloadId == null || downloadId == newDownloadId)
                {
                    downloadId = newDownloadId;
                }
                else
                {
                    return null;
                }
            }

            return downloadId;
        }

        public void Handle(BookGrabbedEvent message)
        {
            foreach (var book in message.Book.Books)
            {
                var history = new History
                {
                    EventType = HistoryEventType.Grabbed,
                    Date = DateTime.UtcNow,
                    Quality = message.Book.ParsedBookInfo.Quality,
                    SourceTitle = message.Book.Release.Title,
                    AuthorId = book.AuthorId,
                    BookId = book.Id,
                    DownloadId = message.DownloadId
                };

                history.Data.Add("Indexer", message.Book.Release.Indexer);
                history.Data.Add("NzbInfoUrl", message.Book.Release.InfoUrl);
                history.Data.Add("ReleaseGroup", message.Book.ParsedBookInfo.ReleaseGroup);
                history.Data.Add("Age", message.Book.Release.Age.ToString());
                history.Data.Add("AgeHours", message.Book.Release.AgeHours.ToString());
                history.Data.Add("AgeMinutes", message.Book.Release.AgeMinutes.ToString());
                history.Data.Add("PublishedDate", message.Book.Release.PublishDate.ToString("s") + "Z");
                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Size", message.Book.Release.Size.ToString());
                history.Data.Add("DownloadUrl", message.Book.Release.DownloadUrl);
                history.Data.Add("Guid", message.Book.Release.Guid);
                history.Data.Add("Protocol", ((int)message.Book.Release.DownloadProtocol).ToString());
                history.Data.Add("DownloadForced", (!message.Book.DownloadAllowed).ToString());

                if (!message.Book.ParsedBookInfo.ReleaseHash.IsNullOrWhiteSpace())
                {
                    history.Data.Add("ReleaseHash", message.Book.ParsedBookInfo.ReleaseHash);
                }

                var torrentRelease = message.Book.Release as TorrentInfo;

                if (torrentRelease != null)
                {
                    history.Data.Add("TorrentInfoHash", torrentRelease.InfoHash);
                }

                _historyRepository.Insert(history);
            }
        }

        public void Handle(BookImportIncompleteEvent message)
        {
            foreach (var book in message.TrackedDownload.RemoteBook.Books)
            {
                var history = new History
                {
                    EventType = HistoryEventType.BookImportIncomplete,
                    Date = DateTime.UtcNow,
                    Quality = message.TrackedDownload.RemoteBook.ParsedBookInfo?.Quality ?? new QualityModel(),
                    SourceTitle = message.TrackedDownload.DownloadItem.Title,
                    AuthorId = book.AuthorId,
                    BookId = book.Id,
                    DownloadId = message.TrackedDownload.DownloadItem.DownloadId
                };

                history.Data.Add("StatusMessages", message.TrackedDownload.StatusMessages.ToJson());
                _historyRepository.Insert(history);
            }
        }

        public void Handle(TrackImportedEvent message)
        {
            if (!message.NewDownload)
            {
                return;
            }

            var downloadId = message.DownloadId;

            if (downloadId.IsNullOrWhiteSpace())
            {
                downloadId = FindDownloadId(message);
            }

            var history = new History
            {
                EventType = HistoryEventType.BookFileImported,
                Date = DateTime.UtcNow,
                Quality = message.BookInfo.Quality,
                SourceTitle = message.ImportedBook.SceneName ?? Path.GetFileNameWithoutExtension(message.BookInfo.Path),
                AuthorId = message.BookInfo.Author.Id,
                BookId = message.BookInfo.Book.Id,
                DownloadId = downloadId
            };

            //Won't have a value since we publish this event before saving to DB.
            //history.Data.Add("FileId", message.ImportedEpisode.Id.ToString());
            history.Data.Add("DroppedPath", message.BookInfo.Path);
            history.Data.Add("ImportedPath", message.ImportedBook.Path);
            history.Data.Add("DownloadClient", message.DownloadClient);

            _historyRepository.Insert(history);
        }

        public void Handle(DownloadFailedEvent message)
        {
            foreach (var bookId in message.BookIds)
            {
                var history = new History
                {
                    EventType = HistoryEventType.DownloadFailed,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    AuthorId = message.AuthorId,
                    BookId = bookId,
                    DownloadId = message.DownloadId
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Message", message.Message);

                _historyRepository.Insert(history);
            }
        }

        public void Handle(DownloadCompletedEvent message)
        {
            foreach (var book in message.TrackedDownload.RemoteBook.Books)
            {
                var history = new History
                {
                    EventType = HistoryEventType.DownloadImported,
                    Date = DateTime.UtcNow,
                    Quality = message.TrackedDownload.RemoteBook.ParsedBookInfo?.Quality ?? new QualityModel(),
                    SourceTitle = message.TrackedDownload.DownloadItem.Title,
                    AuthorId = book.AuthorId,
                    BookId = book.Id,
                    DownloadId = message.TrackedDownload.DownloadItem.DownloadId
                };

                _historyRepository.Insert(history);
            }
        }

        public void Handle(BookFileDeletedEvent message)
        {
            if (message.Reason == DeleteMediaFileReason.NoLinkedEpisodes)
            {
                _logger.Debug("Removing book file from DB as part of cleanup routine, not creating history event.");
                return;
            }
            else if (message.Reason == DeleteMediaFileReason.ManualOverride)
            {
                _logger.Debug("Removing book file from DB as part of manual override of existing file, not creating history event.");
                return;
            }

            var history = new History
            {
                EventType = HistoryEventType.BookFileDeleted,
                Date = DateTime.UtcNow,
                Quality = message.BookFile.Quality,
                SourceTitle = message.BookFile.Path,
                AuthorId = message.BookFile.Author.Value.Id,
                BookId = message.BookFile.BookId
            };

            history.Data.Add("Reason", message.Reason.ToString());

            _historyRepository.Insert(history);
        }

        public void Handle(BookFileRenamedEvent message)
        {
            var sourcePath = message.OriginalPath;
            var path = message.BookFile.Path;

            var history = new History
            {
                EventType = HistoryEventType.BookFileRenamed,
                Date = DateTime.UtcNow,
                Quality = message.BookFile.Quality,
                SourceTitle = message.OriginalPath,
                AuthorId = message.BookFile.Author.Value.Id,
                BookId = message.BookFile.BookId
            };

            history.Data.Add("SourcePath", sourcePath);
            history.Data.Add("Path", path);

            _historyRepository.Insert(history);
        }

        public void Handle(BookFileRetaggedEvent message)
        {
            var path = message.BookFile.Path;

            var history = new History
            {
                EventType = HistoryEventType.BookFileRetagged,
                Date = DateTime.UtcNow,
                Quality = message.BookFile.Quality,
                SourceTitle = path,
                AuthorId = message.BookFile.Author.Value.Id,
                BookId = message.BookFile.BookId
            };

            history.Data.Add("TagsScrubbed", message.Scrubbed.ToString());
            history.Data.Add("Diff", message.Diff.Select(x => new
            {
                Field = x.Key,
                OldValue = x.Value.Item1,
                NewValue = x.Value.Item2
            }).ToJson());

            _historyRepository.Insert(history);
        }

        public void Handle(AuthorDeletedEvent message)
        {
            _historyRepository.DeleteForAuthor(message.Author.Id);
        }

        public void Handle(DownloadIgnoredEvent message)
        {
            var historyToAdd = new List<History>();
            foreach (var bookId in message.BookIds)
            {
                var history = new History
                {
                    EventType = HistoryEventType.DownloadIgnored,
                    Date = DateTime.UtcNow,
                    Quality = message.Quality,
                    SourceTitle = message.SourceTitle,
                    AuthorId = message.AuthorId,
                    BookId = bookId,
                    DownloadId = message.DownloadId
                };

                history.Data.Add("DownloadClient", message.DownloadClient);
                history.Data.Add("Message", message.Message);

                historyToAdd.Add(history);
            }

            _historyRepository.InsertMany(historyToAdd);
        }

        public List<History> Since(DateTime date, HistoryEventType? eventType)
        {
            return _historyRepository.Since(date, eventType);
        }

        public void UpdateMany(IList<History> items)
        {
            _historyRepository.UpdateMany(items);
        }
    }
}
