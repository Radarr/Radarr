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

            localMovie.Release = new GrabbedReleaseInfo(grabbedHistories);

            return localMovie;
        }
    }
}
