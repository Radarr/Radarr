using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadService
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
        private readonly IConfigService _config;
        private readonly Logger _logger;
        private readonly ICached<TrackedDownload> _cache;

        public TrackedDownloadService(IParsingService parsingService,
            ICacheManager cacheManager,
            IHistoryService historyService,
            IConfigService config,
            IEventAggregator eventAggregator,
            Logger logger)
        {
            _parsingService = parsingService;
            _historyService = historyService;
            _cache = cacheManager.GetCache<TrackedDownload>(GetType());
            _config = config;
            _eventAggregator = eventAggregator;
            _logger = logger;
        }

        public TrackedDownload Find(string downloadId)
        {
            return _cache.Find(downloadId);
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
            if (downloadItem.DownloadId.IsNullOrWhiteSpace())
            {
                _logger.Warn("The following download client item ({0}) has no download hash (id), so it cannot be tracked: {1}", 
                    downloadClient.Name, downloadItem.Title);
                return null;
            }

            if (downloadItem.Title.IsNullOrWhiteSpace())
            {
                _logger.Warn("The following download client item ({0}) has no title so it cannot be tracked: {1}",
                    downloadClient.Name, downloadItem.Title);
                return null;
            }
            
            var existingItem = Find(downloadItem.DownloadId);

            if (existingItem != null && existingItem.State != TrackedDownloadStage.Downloading)
            {
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

                var historyItems = _historyService.FindByDownloadId(downloadItem.DownloadId);
                var grabbedHistoryItem = historyItems.OrderByDescending(h => h.Date).FirstOrDefault(h => h.EventType == HistoryEventType.Grabbed);
                var firstHistoryItem = historyItems.OrderByDescending(h => h.Date).FirstOrDefault();
                //TODO: Create release info from history and use that here, so we don't loose indexer flags!
                var parsedMovieInfo = _parsingService.ParseMovieInfo(trackedDownload.DownloadItem.Title, new List<object>{grabbedHistoryItem});

                if (parsedMovieInfo != null)
                {
                    trackedDownload.RemoteMovie = _parsingService.Map(parsedMovieInfo, "", null).RemoteMovie;
                }

                if (firstHistoryItem != null)
                {
                    trackedDownload.State = GetStateFromHistory(firstHistoryItem.EventType);

                    var grabbedEvent = historyItems.FirstOrDefault(v => v.EventType == HistoryEventType.Grabbed);
                    trackedDownload.Indexer = grabbedEvent?.Data["indexer"];

                    if (parsedMovieInfo == null ||
                        trackedDownload.RemoteMovie == null ||
                        trackedDownload.RemoteMovie.Movie == null)
                    {
                        parsedMovieInfo = _parsingService.ParseMovieInfo(firstHistoryItem.SourceTitle, new List<object>{grabbedHistoryItem});

                        if (parsedMovieInfo != null)
                        {
                            trackedDownload.RemoteMovie = _parsingService.Map(parsedMovieInfo, "", null).RemoteMovie;
                        }
                    }
                }

                if (trackedDownload.RemoteMovie == null)
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Failed to find movie for " + downloadItem.Title);
                return null;
            }

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
                _logger.Debug("Tracking '{0}:{1}': ClientState={2}{3} SonarrStage={4} Episode='{5}' OutputPath={6}.",
                    downloadItem.DownloadClient, downloadItem.Title,
                    downloadItem.Status, downloadItem.CanBeRemoved ? "" :
                                         downloadItem.CanMoveFiles ? " (busy)" : " (readonly)",
                    trackedDownload.State,
                    trackedDownload.RemoteMovie?.ParsedMovieInfo,
                    downloadItem.OutputPath);
            }
        }

        private static TrackedDownloadStage GetStateFromHistory(HistoryEventType eventType)
        {
            switch (eventType)
            {
                case HistoryEventType.DownloadFolderImported:
                    return TrackedDownloadStage.Imported;
                case HistoryEventType.DownloadFailed:
                    return TrackedDownloadStage.DownloadFailed;
                default:
                    return TrackedDownloadStage.Downloading;
            }
        }
    }
}
