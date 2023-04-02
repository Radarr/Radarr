using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Download;
using NzbDrone.Core.History;
using NzbDrone.Core.Parser.Model;

namespace NzbDrone.Core.MediaFiles.MovieImport.Aggregation.Aggregators
{
    public class AggregateReleaseInfo : IAggregateLocalMovie
    {
        private readonly IHistoryService _historyService;

        public AggregateReleaseInfo(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        public LocalMovie Aggregate(LocalMovie localMovie, DownloadClientItem downloadClientItem)
        {
            if (downloadClientItem == null)
            {
                return localMovie;
            }

            var grabbedHistories = _historyService.FindByDownloadId(downloadClientItem.DownloadId)
                .Where(h => h.EventType == MovieHistoryEventType.Grabbed)
                .ToList();

            if (grabbedHistories.Empty())
            {
                return localMovie;
            }

            var movieIds = grabbedHistories.Select(h => h.MovieId).Distinct().ToList();
            var grabbedHistory = grabbedHistories.First();
            var releaseInfo = new GrabbedReleaseInfo();

            grabbedHistory.Data.TryGetValue("indexer", out var indexer);
            grabbedHistory.Data.TryGetValue("size", out var sizeString);
            long.TryParse(sizeString, out var size);

            releaseInfo.Title = grabbedHistory.SourceTitle;
            releaseInfo.Indexer = indexer;
            releaseInfo.Size = size;
            releaseInfo.MovieIds = movieIds;

            localMovie.Release = releaseInfo;

            return localMovie;
        }
    }
}
