using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Books;
using NzbDrone.Core.Books.Events;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadService : IHandle<BookDeletedEvent>
    {
        TrackedDownload Find(string downloadId);
        void StopTracking(string downloadId);
        void StopTracking(List<string> downloadIds);
        TrackedDownload TrackDownload(DownloadClientDefinition downloadClient, DownloadClientItem downloadItem);
        List<TrackedDownload> GetTrackedDownloads();
        void UpdateTrackable(List<TrackedDownload> trackedDownloads);
    }

    public class TrackedDownloadService : ITrackedDownloadService
    {
        private readonly IParsingService _parsingService;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITrackedDownloadAlreadyImported _trackedDownloadAlreadyImported;
        private readonly Logger _logger;
        private readonly ICached<TrackedDownload> _cache;

        public TrackedDownloadService(IParsingService parsingService,
                                      ICacheManager cacheManager,
                                      IHistoryService historyService,
                                      IEventAggregator eventAggregator,
                                      ITrackedDownloadAlreadyImported trackedDownloadAlreadyImported,
                                      Logger logger)
        {
            _parsingService = parsingService;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _trackedDownloadAlreadyImported = trackedDownloadAlreadyImported;
            _cache = cacheManager.GetCache<TrackedDownload>(GetType());
            _logger = logger;
        }

        public TrackedDownload Find(string downloadId)
        {
            return _cache.Find(downloadId);
        }

        public void UpdateAlbumCache(int bookId)
        {
            var updateCacheItems = _cache.Values.Where(x => x.RemoteBook != null && x.RemoteBook.Books.Any(a => a.Id == bookId)).ToList();

            foreach (var item in updateCacheItems)
            {
                var parsedBookInfo = Parser.Parser.ParseBookTitle(item.DownloadItem.Title);
                item.RemoteBook = null;

                if (parsedBookInfo != null)
                {
                    item.RemoteBook = _parsingService.Map(parsedBookInfo);
                }
            }

            _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(GetTrackedDownloads()));
        }

        public void StopTracking(string downloadId)
        {
            var trackedDownload = _cache.Find(downloadId);

            _cache.Remove(downloadId);
            _eventAggregator.PublishEvent(new TrackedDownloadsRemovedEvent(new List<TrackedDownload> { trackedDownload }));
        }

        public void StopTracking(List<string> downloadIds)
        {
            var trackedDownloads = new List<TrackedDownload>();

            foreach (var downloadId in downloadIds)
            {
                var trackedDownload = _cache.Find(downloadId);
                _cache.Remove(downloadId);
                trackedDownloads.Add(trackedDownload);
            }

            _eventAggregator.PublishEvent(new TrackedDownloadsRemovedEvent(trackedDownloads));
        }

        public TrackedDownload TrackDownload(DownloadClientDefinition downloadClient, DownloadClientItem downloadItem)
        {
            var existingItem = Find(downloadItem.DownloadId);

            if (existingItem != null && existingItem.State != TrackedDownloadState.Downloading)
            {
                LogItemChange(existingItem, existingItem.DownloadItem, downloadItem);

                existingItem.DownloadItem = downloadItem;
                existingItem.IsTrackable = true;

                return existingItem;
            }

            var trackedDownload = new TrackedDownload
            {
                DownloadClient = downloadClient.Id,
                DownloadItem = downloadItem,
                Protocol = downloadClient.Protocol,
                IsTrackable = true
            };

            try
            {
                var parsedBookInfo = Parser.Parser.ParseBookTitle(trackedDownload.DownloadItem.Title);
                var historyItems = _historyService.FindByDownloadId(downloadItem.DownloadId)
                    .OrderByDescending(h => h.Date)
                    .ToList();

                if (parsedBookInfo != null)
                {
                    trackedDownload.RemoteBook = _parsingService.Map(parsedBookInfo);
                }

                if (historyItems.Any())
                {
                    var firstHistoryItem = historyItems.First();
                    var state = GetStateFromHistory(firstHistoryItem);

                    // One potential issue here is if the latest is imported, but other episodes are ignored or never imported.
                    // It's unlikely that will happen, but could happen if additional episodes are added to season after it's already imported.
                    if (state == TrackedDownloadState.Imported)
                    {
                        var allImported = _trackedDownloadAlreadyImported.IsImported(trackedDownload, historyItems);

                        trackedDownload.State = allImported ? TrackedDownloadState.Imported : TrackedDownloadState.Downloading;
                    }
                    else
                    {
                        trackedDownload.State = state;
                    }

                    if (firstHistoryItem.EventType == HistoryEventType.BookImportIncomplete)
                    {
                        var messages = Json.Deserialize<List<TrackedDownloadStatusMessage>>(firstHistoryItem?.Data["statusMessages"]).ToArray();
                        trackedDownload.Warn(messages);
                    }

                    var grabbedEvent = historyItems.FirstOrDefault(v => v.EventType == HistoryEventType.Grabbed);
                    trackedDownload.Indexer = grabbedEvent?.Data["indexer"];

                    if (parsedBookInfo == null ||
                        trackedDownload.RemoteBook == null ||
                        trackedDownload.RemoteBook.Author == null ||
                        trackedDownload.RemoteBook.Books.Empty())
                    {
                        // Try parsing the original source title and if that fails, try parsing it as a special
                        // TODO: Pass the TVDB ID and TVRage IDs in as well so we have a better chance for finding the item
                        var historyAuthor = firstHistoryItem.Author;
                        var historyBooks = new List<Book> { firstHistoryItem.Book };

                        parsedBookInfo = Parser.Parser.ParseBookTitle(firstHistoryItem.SourceTitle);

                        if (parsedBookInfo != null)
                        {
                            trackedDownload.RemoteBook = _parsingService.Map(parsedBookInfo,
                                firstHistoryItem.AuthorId,
                                historyItems.Where(v => v.EventType == HistoryEventType.Grabbed).Select(h => h.BookId)
                                    .Distinct());
                        }
                        else
                        {
                            parsedBookInfo =
                                Parser.Parser.ParseBookTitleWithSearchCriteria(firstHistoryItem.SourceTitle,
                                    historyAuthor,
                                    historyBooks);

                            if (parsedBookInfo != null)
                            {
                                trackedDownload.RemoteBook = _parsingService.Map(parsedBookInfo,
                                    firstHistoryItem.AuthorId,
                                    historyItems.Where(v => v.EventType == HistoryEventType.Grabbed).Select(h => h.BookId)
                                        .Distinct());
                            }
                        }
                    }
                }

                // Track it so it can be displayed in the queue even though we can't determine which author it is for
                if (trackedDownload.RemoteBook == null)
                {
                    _logger.Trace("No Book found for download '{0}'", trackedDownload.DownloadItem.Title);
                    trackedDownload.Warn("No Book found for download '{0}'", trackedDownload.DownloadItem.Title);
                }
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Failed to find book for " + downloadItem.Title);
                return null;
            }

            LogItemChange(trackedDownload, existingItem?.DownloadItem, trackedDownload.DownloadItem);

            _cache.Set(trackedDownload.DownloadItem.DownloadId, trackedDownload);
            return trackedDownload;
        }

        public List<TrackedDownload> GetTrackedDownloads()
        {
            return _cache.Values.ToList();
        }

        public void UpdateTrackable(List<TrackedDownload> trackedDownloads)
        {
            var untrackable = GetTrackedDownloads().ExceptBy(t => t.DownloadItem.DownloadId, trackedDownloads, t => t.DownloadItem.DownloadId, StringComparer.CurrentCulture).ToList();

            foreach (var trackedDownload in untrackable)
            {
                trackedDownload.IsTrackable = false;
            }
        }

        private void LogItemChange(TrackedDownload trackedDownload, DownloadClientItem existingItem, DownloadClientItem downloadItem)
        {
            if (existingItem == null ||
                existingItem.Status != downloadItem.Status ||
                existingItem.CanBeRemoved != downloadItem.CanBeRemoved ||
                existingItem.CanMoveFiles != downloadItem.CanMoveFiles)
            {
                _logger.Debug("Tracking '{0}:{1}': ClientState={2}{3} ReadarrStage={4} Book='{5}' OutputPath={6}.",
                    downloadItem.DownloadClient,
                    downloadItem.Title,
                    downloadItem.Status,
                    downloadItem.CanBeRemoved ? "" : downloadItem.CanMoveFiles ? " (busy)" : " (readonly)",
                    trackedDownload.State,
                    trackedDownload.RemoteBook?.ParsedBookInfo,
                    downloadItem.OutputPath);
            }
        }

        private static TrackedDownloadState GetStateFromHistory(NzbDrone.Core.History.History history)
        {
            switch (history.EventType)
            {
                case HistoryEventType.BookImportIncomplete:
                    return TrackedDownloadState.ImportFailed;
                case HistoryEventType.DownloadImported:
                    return TrackedDownloadState.Imported;
                case HistoryEventType.DownloadFailed:
                    return TrackedDownloadState.DownloadFailed;
                case HistoryEventType.DownloadIgnored:
                    return TrackedDownloadState.Ignored;
            }

            // Since DownloadComplete is a new event type, we can't assume it exists for old downloads
            if (history.EventType == HistoryEventType.BookFileImported)
            {
                return DateTime.UtcNow.Subtract(history.Date).TotalSeconds < 60 ? TrackedDownloadState.Importing : TrackedDownloadState.Imported;
            }

            return TrackedDownloadState.Downloading;
        }

        public void Handle(BookDeletedEvent message)
        {
            UpdateAlbumCache(message.Book.Id);
        }
    }
}
