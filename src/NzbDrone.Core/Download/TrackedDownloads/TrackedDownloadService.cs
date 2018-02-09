using System;
using System.Linq;
using NLog;
using NzbDrone.Common.Cache;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Configuration;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser;

namespace NzbDrone.Core.Download.TrackedDownloads
{
    public interface ITrackedDownloadService
    {
        TrackedDownload Find(string downloadId);
        TrackedDownload TrackDownload(DownloadClientDefinition downloadClient, DownloadClientItem downloadItem);
    }

    public class TrackedDownloadService : ITrackedDownloadService
    {
        private readonly IParsingService _parsingService;
        private readonly IHistoryService _historyService;
        private readonly IConfigService _config;
        private readonly Logger _logger;
        private readonly ICached<TrackedDownload> _cache;

        public TrackedDownloadService(IParsingService parsingService,
            ICacheManager cacheManager,
            IHistoryService historyService,
            IConfigService config,
            Logger logger)
        {
            _parsingService = parsingService;
            _historyService = historyService;
            _cache = cacheManager.GetCache<TrackedDownload>(GetType());
            _config = config;
            _logger = logger;
        }

        public TrackedDownload Find(string downloadId)
        {
            return _cache.Find(downloadId);
        }

        public TrackedDownload TrackDownload(DownloadClientDefinition downloadClient, DownloadClientItem downloadItem)
        {
            var existingItem = Find(downloadItem.DownloadId);

            if (existingItem != null && existingItem.State != TrackedDownloadStage.Downloading)
            {
                existingItem.DownloadItem = downloadItem;
                return existingItem;
            }

            var trackedDownload = new TrackedDownload
            {
                DownloadClient = downloadClient.Id,
                DownloadItem = downloadItem,
                Protocol = downloadClient.Protocol
            };

            try
            {
                
                var parsedMovieInfo = Parser.Parser.ParseMovieTitle(trackedDownload.DownloadItem.Title, _config.ParsingLeniency > 0);
                var historyItems = _historyService.FindByDownloadId(downloadItem.DownloadId);

                if (parsedMovieInfo != null)
                {
                    trackedDownload.RemoteMovie = _parsingService.Map(parsedMovieInfo, "", null).RemoteMovie;
                }

                if (historyItems.Any())
                {
                    var firstHistoryItem = historyItems.OrderByDescending(h => h.Date).First();
                    trackedDownload.State = GetStateFromHistory(firstHistoryItem.EventType);

                    if (parsedMovieInfo == null ||
                        trackedDownload.RemoteMovie == null ||
                        trackedDownload.RemoteMovie.Movie == null)
                    {
                        parsedMovieInfo = Parser.Parser.ParseMovieTitle(firstHistoryItem.SourceTitle, _config.ParsingLeniency > 0);

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
                _logger.Debug(e, "Failed to find episode for " + downloadItem.Title);
                return null;
            }

            _cache.Set(trackedDownload.DownloadItem.DownloadId, trackedDownload);
            return trackedDownload;
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
