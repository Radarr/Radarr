using System;
using System.Collections.Generic;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Download.Aggregation;
using NzbDrone.Core.Download.History;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Movies.Events;
using NzbDrone.Core.Parser;
using NzbDrone.Core.Parser.Model;

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

    public class TrackedDownloadService : ITrackedDownloadService,
                                          IHandle<MovieAddedEvent>,
                                          IHandle<MoviesDeletedEvent>
    {
        private readonly IParsingService _parsingService;
        private readonly IHistoryService _historyService;
        private readonly IEventAggregator _eventAggregator;
        private readonly IDownloadHistoryService _downloadHistoryService;
        private readonly IConfigService _config;
        private readonly IRemoteMovieAggregationService _aggregationService;
        private readonly ICustomFormatCalculationService _formatCalculator;
        private readonly Logger _logger;
        private readonly ICached<TrackedDownload> _cache;

        public TrackedDownloadService(IParsingService parsingService,
                                      ICacheManager cacheManager,
                                      IHistoryService historyService,
                                      IConfigService config,
                                      IRemoteMovieAggregationService aggregationService,
                                      ICustomFormatCalculationService formatCalculator,
                                      IEventAggregator eventAggregator,
                                      IDownloadHistoryService downloadHistoryService,
                                      Logger logger)
        {
            _parsingService = parsingService;
            _historyService = historyService;
            _cache = cacheManager.GetCache<TrackedDownload>(GetType());
            _config = config;
            _aggregationService = aggregationService;
            _formatCalculator = formatCalculator;
            _eventAggregator = eventAggregator;
            _downloadHistoryService = downloadHistoryService;
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
                IsTrackable = true,
                HasNotifiedManualInteractionRequired = existingItem?.HasNotifiedManualInteractionRequired ?? false
            };

            try
            {
                var historyItems = _historyService.FindByDownloadId(downloadItem.DownloadId)
                    .OrderByDescending(h => h.Date)
                    .ToList();

                var parsedMovieInfo = Parser.Parser.ParseMovieTitle(trackedDownload.DownloadItem.Title);

                if (parsedMovieInfo != null)
                {
                    trackedDownload.RemoteMovie = _parsingService.Map(parsedMovieInfo, "", 0, null);

                    _aggregationService.Augment(trackedDownload.RemoteMovie);
                }

                var downloadHistory = _downloadHistoryService.GetLatestDownloadHistoryItem(downloadItem.DownloadId);

                if (downloadHistory != null)
                {
                    var state = GetStateFromHistory(downloadHistory.EventType);
                    trackedDownload.State = state;
                }

                if (historyItems.Any())
                {
                    var firstHistoryItem = historyItems.First();
                    var grabbedEvent = historyItems.FirstOrDefault(v => v.EventType == MovieHistoryEventType.Grabbed);

                    trackedDownload.Indexer = grabbedEvent?.Data?.GetValueOrDefault("indexer");
                    trackedDownload.Added = grabbedEvent?.Date;

                    if (parsedMovieInfo == null ||
                        trackedDownload.RemoteMovie?.Movie == null)
                    {
                        parsedMovieInfo = Parser.Parser.ParseMovieTitle(firstHistoryItem.SourceTitle);

                        if (parsedMovieInfo != null)
                        {
                            trackedDownload.RemoteMovie = _parsingService.Map(parsedMovieInfo,
                                firstHistoryItem.MovieId);
                        }
                    }

                    if (trackedDownload.RemoteMovie != null &&
                        Enum.TryParse(grabbedEvent?.Data?.GetValueOrDefault("indexerFlags"), true, out IndexerFlags flags))
                    {
                        trackedDownload.RemoteMovie.Release ??= new ReleaseInfo();
                        trackedDownload.RemoteMovie.Release.IndexerFlags = flags;
                    }
                }

                // Calculate custom formats
                if (trackedDownload.RemoteMovie != null)
                {
                    trackedDownload.RemoteMovie.CustomFormats = _formatCalculator.ParseCustomFormat(trackedDownload.RemoteMovie, downloadItem.TotalSize);
                }

                // Track it so it can be displayed in the queue even though we can't determine which movie it is for
                if (trackedDownload.RemoteMovie == null)
                {
                    _logger.Trace("No Movie found for download '{0}'", trackedDownload.DownloadItem.Title);
                }
            }
            catch (MultipleMoviesFoundException e)
            {
                _logger.Debug(e, "Found multiple movies for " + downloadItem.Title);

                trackedDownload.Warn("Unable to import automatically, found multiple movies: {0}", string.Join(", ", e.Movies));
            }
            catch (Exception e)
            {
                _logger.Debug(e, "Failed to find movie for " + downloadItem.Title);
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

        private void UpdateCachedItem(TrackedDownload trackedDownload)
        {
            var parsedMovieInfo = Parser.Parser.ParseMovieTitle(trackedDownload.DownloadItem.Title);

            trackedDownload.RemoteMovie = parsedMovieInfo == null ? null : _parsingService.Map(parsedMovieInfo, "", 0, null);

            _aggregationService.Augment(trackedDownload.RemoteMovie);
        }

        private static TrackedDownloadState GetStateFromHistory(DownloadHistoryEventType eventType)
        {
            switch (eventType)
            {
                case DownloadHistoryEventType.DownloadImported:
                    return TrackedDownloadState.Imported;
                case DownloadHistoryEventType.DownloadFailed:
                    return TrackedDownloadState.Failed;
                case DownloadHistoryEventType.DownloadIgnored:
                    return TrackedDownloadState.Ignored;
                default:
                    return TrackedDownloadState.Downloading;
            }
        }

        private void LogItemChange(TrackedDownload trackedDownload, DownloadClientItem existingItem, DownloadClientItem downloadItem)
        {
            if (existingItem == null ||
                existingItem.Status != downloadItem.Status ||
                existingItem.CanBeRemoved != downloadItem.CanBeRemoved ||
                 existingItem.CanMoveFiles != downloadItem.CanMoveFiles)
            {
                _logger.Debug("Tracking '{0}:{1}': ClientState={2}{3} RadarrStage={4} Movie='{5}' OutputPath={6}.",
                    downloadItem.DownloadClientInfo.Name,
                    downloadItem.Title,
                    downloadItem.Status,
                    downloadItem.CanBeRemoved ? "" : downloadItem.CanMoveFiles ? " (busy)" : " (readonly)",
                    trackedDownload.State,
                    trackedDownload.RemoteMovie?.ParsedMovieInfo,
                    downloadItem.OutputPath);
            }
        }

        public void Handle(MovieAddedEvent message)
        {
            var cachedItems = _cache.Values
                .Where(t =>
                    t.RemoteMovie?.Movie == null ||
                    message.Movie?.TmdbId == t.RemoteMovie.Movie.TmdbId)
                .ToList();

            if (cachedItems.Any())
            {
                cachedItems.ForEach(UpdateCachedItem);

                _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(GetTrackedDownloads()));
            }
        }

        public void Handle(MoviesDeletedEvent message)
        {
            var cachedItems = _cache.Values
                .Where(t =>
                    t.RemoteMovie?.Movie != null &&
                    message.Movies.Any(m => m.Id == t.RemoteMovie.Movie.Id || m.TmdbId == t.RemoteMovie.Movie.TmdbId))
                .ToList();

            if (cachedItems.Any())
            {
                cachedItems.ForEach(UpdateCachedItem);

                _eventAggregator.PublishEvent(new TrackedDownloadRefreshedEvent(GetTrackedDownloads()));
            }
        }
    }
}
