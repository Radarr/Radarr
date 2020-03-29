using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Download
{
    public interface IFailedDownloadService
    {
        void MarkAsFailed(int historyId);
        void MarkAsFailed(string downloadId);
        void Check(TrackedDownload trackedDownload);
        void ProcessFailed(TrackedDownload trackedDownload);
    }

    public class FailedDownloadService : IFailedDownloadService
    {
        private readonly IHistoryService _historyService;
        private readonly ITrackedDownloadService _trackedDownloadService;
        private readonly IEventAggregator _eventAggregator;

        public FailedDownloadService(IHistoryService historyService,
                                     ITrackedDownloadService trackedDownloadService,
                                     IEventAggregator eventAggregator)
        {
            _historyService = historyService;
            _trackedDownloadService = trackedDownloadService;
            _eventAggregator = eventAggregator;
        }

        public void MarkAsFailed(int historyId)
        {
            var history = _historyService.Get(historyId);

            var downloadId = history.DownloadId;
            if (downloadId.IsNullOrWhiteSpace())
            {
                PublishDownloadFailedEvent(new List<MovieHistory> { history }, "Manually marked as failed");
            }
            else
            {
                var grabbedHistory = _historyService.Find(downloadId, MovieHistoryEventType.Grabbed).ToList();
                PublishDownloadFailedEvent(grabbedHistory, "Manually marked as failed");
            }
        }

        public void MarkAsFailed(string downloadId)
        {
            var history = _historyService.Find(downloadId, MovieHistoryEventType.Grabbed);

            if (history.Any())
            {
                var trackedDownload = _trackedDownloadService.Find(downloadId);

                PublishDownloadFailedEvent(history, "Manually marked as failed", trackedDownload);
            }
        }

        public void Check(TrackedDownload trackedDownload)
        {
            // Only process tracked downloads that are still downloading
            if (trackedDownload.State != TrackedDownloadState.Downloading)
            {
                return;
            }

            if (trackedDownload.DownloadItem.IsEncrypted ||
                trackedDownload.DownloadItem.Status == DownloadItemStatus.Failed)
            {
                var grabbedItems = _historyService
                                   .Find(trackedDownload.DownloadItem.DownloadId, MovieHistoryEventType.Grabbed)
                                   .ToList();

                if (grabbedItems.Empty())
                {
                    trackedDownload.Warn("Download wasn't grabbed by Radarr, skipping");
                    return;
                }

                trackedDownload.State = TrackedDownloadState.FailedPending;
            }
        }

        public void ProcessFailed(TrackedDownload trackedDownload)
        {
            if (trackedDownload.State != TrackedDownloadState.FailedPending)
            {
                return;
            }

            var grabbedItems = _historyService
                               .Find(trackedDownload.DownloadItem.DownloadId, MovieHistoryEventType.Grabbed)
                               .ToList();

            if (grabbedItems.Empty())
            {
                return;
            }

            var failure = "Failed download detected";

            if (trackedDownload.DownloadItem.IsEncrypted)
            {
                failure = "Encrypted download detected";
            }
            else if (trackedDownload.DownloadItem.Status == DownloadItemStatus.Failed && trackedDownload.DownloadItem.Message.IsNotNullOrWhiteSpace())
            {
                failure = trackedDownload.DownloadItem.Message;
            }

            trackedDownload.State = TrackedDownloadState.Failed;
            PublishDownloadFailedEvent(grabbedItems, failure, trackedDownload);
        }

        private void PublishDownloadFailedEvent(List<MovieHistory> historyItems, string message, TrackedDownload trackedDownload = null)
        {
            var historyItem = historyItems.First();

            var downloadFailedEvent = new DownloadFailedEvent
            {
                MovieId = historyItem.MovieId,
                Quality = historyItem.Quality,
                SourceTitle = historyItem.SourceTitle,
                DownloadClient = historyItem.Data.GetValueOrDefault(MovieHistory.DOWNLOAD_CLIENT),
                DownloadId = historyItem.DownloadId,
                Message = message,
                Data = historyItem.Data,
                TrackedDownload = trackedDownload,
                Languages = historyItem.Languages
            };

            _eventAggregator.PublishEvent(downloadFailedEvent);
        }
    }
}
