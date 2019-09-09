using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;
using NzbDrone.Core.Parser;
using NzbDrone.Common.Serializer;
using NzbDrone.Core.Music.Events;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadService : IHandle<AlbumDeletedEvent>
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
        private readonly Logger _logger;
        private readonly ICached<TrackedDownload> _cache;

        public TrackedDownloadService(IParsingService parsingService,
                                      ICacheManager cacheManager,
                                      IHistoryService historyService,
                                      IEventAggregator eventAggregator,
                                      Logger logger)
        {
            _parsingService = parsingService;
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _cache = cacheManager.GetCache<TrackedDownload>(GetType());
            _logger = logger;
        }

        public TrackedDownload Find(string downloadId)
        {
            return _cache.Find(downloadId);
        }

        public void UpdateAlbumCache(int albumId)
        {

            var updateCacheItems = _cache.Values.Where(x => x.RemoteAlbum != null && x.RemoteAlbum.Albums.Any(a => a.Id == albumId)).ToList();

            foreach (var item in updateCacheItems)
            {
                var parsedAlbumInfo = Parser.Parser.ParseAlbumTitle(item.DownloadItem.Title);
                item.RemoteAlbum = null;

                if (parsedAlbumInfo != null)
                {
                    item.RemoteAlbum = _parsingService.Map(parsedAlbumInfo);
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

            if (existingItem != null && existingItem.State != TrackedDownloadStage.Downloading)
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
                var parsedAlbumInfo = Parser.Parser.ParseAlbumTitle(trackedDownload.DownloadItem.Title);
                var historyItems = _historyService.FindByDownloadId(downloadItem.DownloadId);

                if (parsedAlbumInfo != null)
                {
                    trackedDownload.RemoteAlbum = _parsingService.Map(parsedAlbumInfo);
                }

                if (historyItems.Any())
                {
                    var firstHistoryItem = historyItems.OrderByDescending(h => h.Date).First();
                    trackedDownload.State = GetStateFromHistory(firstHistoryItem);
                    if (firstHistoryItem.EventType == HistoryEventType.AlbumImportIncomplete)
                    {
                        var messages = Json.Deserialize<List<TrackedDownloadStatusMessage>>(firstHistoryItem?.Data["statusMessages"]).ToArray();
                        trackedDownload.Warn(messages);
                    }

                    var grabbedEvent = historyItems.FirstOrDefault(v => v.EventType == HistoryEventType.Grabbed);
                    trackedDownload.Indexer = grabbedEvent?.Data["indexer"];


                    if (parsedAlbumInfo == null ||
                        trackedDownload.RemoteAlbum == null ||
                        trackedDownload.RemoteAlbum.Artist == null ||
                        trackedDownload.RemoteAlbum.Albums.Empty())
                    {
                        // Try parsing the original source title and if that fails, try parsing it as a special
                        // TODO: Pass the TVDB ID and TVRage IDs in as well so we have a better chance for finding the item
                        var historyArtist = firstHistoryItem.Artist;
                        var historyAlbums = new List<Album> { firstHistoryItem.Album };

                        parsedAlbumInfo = Parser.Parser.ParseAlbumTitle(firstHistoryItem.SourceTitle);

                        if (parsedAlbumInfo != null)
                        {
                            trackedDownload.RemoteAlbum = _parsingService.Map(parsedAlbumInfo,
                                firstHistoryItem.ArtistId,
                                historyItems.Where(v => v.EventType == HistoryEventType.Grabbed).Select(h => h.AlbumId)
                                    .Distinct());
                        }
                        else
                        {
                            parsedAlbumInfo =
                                Parser.Parser.ParseAlbumTitleWithSearchCriteria(firstHistoryItem.SourceTitle,
                                    historyArtist, historyAlbums);

                            if (parsedAlbumInfo != null)
                            {
                                trackedDownload.RemoteAlbum = _parsingService.Map(parsedAlbumInfo,
                                    firstHistoryItem.ArtistId,
                                    historyItems.Where(v => v.EventType == HistoryEventType.Grabbed).Select(h => h.AlbumId)
                                        .Distinct());
                            }
                        }
                    }
                }

                // Track it so it can be displayed in the queue even though we can't determine which artist it is for
                if (trackedDownload.RemoteAlbum == null)
                {
                    _logger.Trace("No Album found for download '{0}'", trackedDownload.DownloadItem.Title);
                    trackedDownload.Warn("No Album found for download '{0}'", trackedDownload.DownloadItem.Title);
                }
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Failed to find album for " + downloadItem.Title);
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
                _logger.Debug("Tracking '{0}:{1}': ClientState={2}{3} LidarrStage={4} Album='{5}' OutputPath={6}.",
                    downloadItem.DownloadClient, downloadItem.Title,
                    downloadItem.Status, downloadItem.CanBeRemoved ? "" :
                                         downloadItem.CanMoveFiles ? " (busy)" : " (readonly)",
                    trackedDownload.State,
                    trackedDownload.RemoteAlbum?.ParsedAlbumInfo,
                    downloadItem.OutputPath);
            }
        }


        private static TrackedDownloadStage GetStateFromHistory(NzbDrone.Core.History.History history)
        {
            switch (history.EventType)
            {
                case HistoryEventType.AlbumImportIncomplete:
                    return TrackedDownloadStage.ImportFailed;
                case HistoryEventType.DownloadImported:
                    return TrackedDownloadStage.Imported;
                case HistoryEventType.DownloadFailed:
                    return TrackedDownloadStage.DownloadFailed;
            }

            // Since DownloadComplete is a new event type, we can't assume it exists for old downloads
            if (history.EventType == HistoryEventType.TrackFileImported)
            {
                return DateTime.UtcNow.Subtract(history.Date).TotalSeconds < 60 ? TrackedDownloadStage.Importing : TrackedDownloadStage.Imported;
            }

            return TrackedDownloadStage.Downloading;
        }

        public void Handle(AlbumDeletedEvent message)
        {
            UpdateAlbumCache(message.Album.Id);
        }
    }
}

