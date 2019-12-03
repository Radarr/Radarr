using System;
using System.Linq;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V3.Queue
{
    public class QueueModule : RadarrRestModuleWithSignalR<QueueResource, NzbDrone.Core.Queue.Queue>,
                               IHandle<QueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;

        private readonly QualityModelComparer QUALITY_COMPARER;

        public QueueModule(IBroadcastSignalRMessage broadcastSignalRMessage,
                           IQueueService queueService,
                           IPendingReleaseService pendingReleaseService,
                           ProfileService qualityProfileService)
            : base(broadcastSignalRMessage)
        {
            _queueService = queueService;
            _pendingReleaseService = pendingReleaseService;
            GetResourcePaged = GetQueue;

            QUALITY_COMPARER = new QualityModelComparer(qualityProfileService.GetDefaultProfile(string.Empty));
        }

        private PagingResource<QueueResource> GetQueue(PagingResource<QueueResource> pagingResource)
        {
            var pagingSpec = pagingResource.MapToPagingSpec<QueueResource, NzbDrone.Core.Queue.Queue>("timeleft", SortDirection.Ascending);
            var includeUnknownMovieItems = Request.GetBooleanQueryParameter("includeUnknownMovieItems");
            var includeMovie = Request.GetBooleanQueryParameter("includeMovie");

            return ApplyToPage((spec) => GetQueue(spec, includeUnknownMovieItems), pagingSpec, (q) => MapToResource(q, includeMovie));
        }

        private PagingSpec<NzbDrone.Core.Queue.Queue> GetQueue(PagingSpec<NzbDrone.Core.Queue.Queue> pagingSpec, bool includeUnknownMovieItems)
        {
            var ascending = pagingSpec.SortDirection == SortDirection.Ascending;
            var orderByFunc = GetOrderByFunc(pagingSpec);

            var queue = _queueService.GetQueue();
            var filteredQueue = includeUnknownMovieItems ? queue : queue.Where(q => q.Movie != null);
            var pending = _pendingReleaseService.GetPendingQueue();
            var fullQueue = filteredQueue.Concat(pending).ToList();
            IOrderedEnumerable<NzbDrone.Core.Queue.Queue> ordered;

            if (pagingSpec.SortKey == "timeleft")
            {
                ordered = ascending
                    ? fullQueue.OrderBy(q => q.Timeleft, new TimeleftComparer())
                    : fullQueue.OrderByDescending(q => q.Timeleft, new TimeleftComparer());
            }

            else if (pagingSpec.SortKey == "estimatedCompletionTime")
            {
                ordered = ascending
                    ? fullQueue.OrderBy(q => q.EstimatedCompletionTime, new EstimatedCompletionTimeComparer())
                    : fullQueue.OrderByDescending(q => q.EstimatedCompletionTime,
                        new EstimatedCompletionTimeComparer());
            }

            else if (pagingSpec.SortKey == "protocol")
            {
                ordered = ascending
                    ? fullQueue.OrderBy(q => q.Protocol)
                    : fullQueue.OrderByDescending(q => q.Protocol);
            }

            else if (pagingSpec.SortKey == "indexer")
            {
                ordered = ascending
                    ? fullQueue.OrderBy(q => q.Indexer, StringComparer.InvariantCultureIgnoreCase)
                    : fullQueue.OrderByDescending(q => q.Indexer, StringComparer.InvariantCultureIgnoreCase);
            }

            else if (pagingSpec.SortKey == "downloadClient")
            {
                ordered = ascending
                    ? fullQueue.OrderBy(q => q.DownloadClient, StringComparer.InvariantCultureIgnoreCase)
                    : fullQueue.OrderByDescending(q => q.DownloadClient, StringComparer.InvariantCultureIgnoreCase);
            }

            else if (pagingSpec.SortKey == "quality")
            {
                ordered = ascending
                    ? fullQueue.OrderBy(q => q.Quality, QUALITY_COMPARER)
                    : fullQueue.OrderByDescending(q => q.Quality, QUALITY_COMPARER);
            }

            else
            {
                ordered = ascending ? fullQueue.OrderBy(orderByFunc) : fullQueue.OrderByDescending(orderByFunc);
            }

            ordered = ordered.ThenByDescending(q => q.Size == 0 ? 0 : 100 - q.Sizeleft / q.Size * 100);

            pagingSpec.Records = ordered.Skip((pagingSpec.Page - 1) * pagingSpec.PageSize).Take(pagingSpec.PageSize).ToList();
            pagingSpec.TotalRecords = fullQueue.Count;

            if (pagingSpec.Records.Empty() && pagingSpec.Page > 1)
            {
                pagingSpec.Page = (int)Math.Max(Math.Ceiling((decimal)(pagingSpec.TotalRecords / pagingSpec.PageSize)), 1);
                pagingSpec.Records = ordered.Skip((pagingSpec.Page - 1) * pagingSpec.PageSize).Take(pagingSpec.PageSize).ToList();
            }

            return pagingSpec;
        }

        private Func<NzbDrone.Core.Queue.Queue, Object> GetOrderByFunc(PagingSpec<NzbDrone.Core.Queue.Queue> pagingSpec)
        {
            switch (pagingSpec.SortKey)
            {
                case "status":
                    return q => q.Status;                
                case "movie.sortTitle":
                    return q => q.Movie.SortTitle;
                case "title":
                    return q => q.Title;
                case "language":
                    return q => q.Languages;
                case "quality":
                    return q => q.Quality;
                case "progress":
                    // Avoid exploding if a download's size is 0
                    return q => 100 - q.Sizeleft / Math.Max(q.Size * 100, 1);
                default:
                    return q => q.Timeleft;
            }
        }

        private QueueResource MapToResource(NzbDrone.Core.Queue.Queue queueItem, bool includeMovie)
        {
            return queueItem.ToResource(includeMovie);
        }

        public void Handle(QueueUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }

        public void Handle(PendingReleasesUpdatedEvent message)
        {
            BroadcastResourceChange(ModelAction.Sync);
        }
    }
}
