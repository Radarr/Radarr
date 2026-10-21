using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.Download
{
    public interface IFailedDownloadService
    {
        void MarkAsFailed(int historyId, bool skipRedownload = false);
        void MarkAsFailed(TrackedDownload trackedDownload, bool skipRedownload = false);
        void Check(TrackedDownload trackedDownload);
        void ProcessFailed(TrackedDownload trackedDownload);
    }

    public class FailedDownloadService : IFailedDownloadService
    {
        // Past this share of its data a torrent is slow rather than dead: failing it would
        // throw away real progress, so the stalled timeout leaves it alone.
        private const double StalledTorrentMaxProgress = 0.05;

        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigService _configService;

        public FailedDownloadService(IHistoryService historyService,
                                     IEventAggregator eventAggregator,
                                     IConfigService configService)
        {
            _historyService = historyService;
            _eventAggregator = eventAggregator;
            _configService = configService;
        }

        public void MarkAsFailed(int historyId, bool skipRedownload = false)
        {
            var history = _historyService.Get(historyId);

            var downloadId = history.DownloadId;

            if (downloadId.IsNullOrWhiteSpace())
            {
                PublishDownloadFailedEvent(history, "Manually marked as failed", skipRedownload: skipRedownload);

                return;
            }

            PublishDownloadFailedEvent(history, "Manually marked as failed");
        }

        public void MarkAsFailed(TrackedDownload trackedDownload, bool skipRedownload = false)
        {
            var history = GetGrabbedHistory(trackedDownload.DownloadItem.DownloadId);

            if (history.Any())
            {
                PublishDownloadFailedEvent(history.First(), "Manually marked as failed", trackedDownload, skipRedownload: skipRedownload);
            }
        }

        public void Check(TrackedDownload trackedDownload)
        {
            // Only process tracked downloads that are still downloading or import is blocked (if they fail after attempting to be processed)
            if (trackedDownload.State != TrackedDownloadState.Downloading && trackedDownload.State != TrackedDownloadState.ImportBlocked)
            {
                return;
            }

            if (trackedDownload.DownloadItem.IsEncrypted ||
                trackedDownload.DownloadItem.Status == DownloadItemStatus.Failed)
            {
                var grabbedItems = GetGrabbedHistory(trackedDownload.DownloadItem.DownloadId);

                if (grabbedItems.Empty())
                {
                    trackedDownload.Warn(trackedDownload.DownloadItem.IsEncrypted ? "Download is encrypted and wasn't grabbed by Radarr, skipping automatic download handling" : "Download has failed wasn't grabbed by Radarr, skipping automatic download handling");
                    return;
                }

                trackedDownload.State = TrackedDownloadState.FailedPending;
                return;
            }

            CheckStalled(trackedDownload);
        }

        public void ProcessFailed(TrackedDownload trackedDownload)
        {
            if (trackedDownload.State != TrackedDownloadState.FailedPending)
            {
                return;
            }

            var grabbedItems = GetGrabbedHistory(trackedDownload.DownloadItem.DownloadId);

            if (grabbedItems.Empty())
            {
                return;
            }

            var failure = "Failed download detected";

            if (trackedDownload.DownloadItem.IsEncrypted)
            {
                failure = "Encrypted download detected";
            }
            else if (trackedDownload.IsStalled)
            {
                failure = "Download stalled with no progress";
            }
            else if (trackedDownload.DownloadItem.Status == DownloadItemStatus.Failed && trackedDownload.DownloadItem.Message.IsNotNullOrWhiteSpace())
            {
                failure = trackedDownload.DownloadItem.Message;
            }

            trackedDownload.State = TrackedDownloadState.Failed;
            PublishDownloadFailedEvent(grabbedItems.First(), failure, trackedDownload);
        }

        private void CheckStalled(TrackedDownload trackedDownload)
        {
            var stalledTorrentTimeout = _configService.StalledTorrentTimeout;

            if (stalledTorrentTimeout == 0 || trackedDownload.Protocol != DownloadProtocol.Torrent)
            {
                return;
            }

            // Paused and queued downloads aren't making progress by design, only consider
            // downloads the client is actively trying to get data for.
            if (trackedDownload.DownloadItem.Status != DownloadItemStatus.Downloading &&
                trackedDownload.DownloadItem.Status != DownloadItemStatus.Warning)
            {
                return;
            }

            if (GetProgress(trackedDownload.DownloadItem) > StalledTorrentMaxProgress)
            {
                return;
            }

            if (!trackedDownload.LastProgressDate.HasValue ||
                trackedDownload.LastProgressDate.Value.AddMinutes(stalledTorrentTimeout) > DateTime.UtcNow)
            {
                return;
            }

            var grabbedItems = GetGrabbedHistory(trackedDownload.DownloadItem.DownloadId);

            if (grabbedItems.Empty())
            {
                trackedDownload.Warn("Download is stalled and wasn't grabbed by Radarr, skipping automatic download handling");
                return;
            }

            trackedDownload.IsStalled = true;
            trackedDownload.State = TrackedDownloadState.FailedPending;
        }

        private static double GetProgress(DownloadClientItem downloadItem)
        {
            // An unknown size means the client has not even fetched the torrent's metadata yet.
            if (downloadItem.TotalSize <= 0)
            {
                return 0;
            }

            return (double)(downloadItem.TotalSize - downloadItem.RemainingSize) / downloadItem.TotalSize;
        }

        private void PublishDownloadFailedEvent(MovieHistory historyItem, string message, TrackedDownload trackedDownload = null, bool skipRedownload = false)
        {
            Enum.TryParse(historyItem.Data.GetValueOrDefault(MovieHistory.RELEASE_SOURCE, ReleaseSourceType.Unknown.ToString()), out ReleaseSourceType releaseSource);

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
                Languages = historyItem.Languages,
                SkipRedownload = skipRedownload,
                ReleaseSource = releaseSource,
            };

            _eventAggregator.PublishEvent(downloadFailedEvent);
        }

        private List<MovieHistory> GetGrabbedHistory(string downloadId)
        {
            // Sort by date so items are always in the same order
            return _historyService.Find(downloadId, MovieHistoryEventType.Grabbed)
                .OrderByDescending(h => h.Date)
                .ToList();
        }
    }
}
