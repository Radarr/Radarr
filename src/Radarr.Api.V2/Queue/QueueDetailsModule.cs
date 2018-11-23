using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.Extensions;

namespace Radarr.Api.V2.Queue
{
    public class QueueDetailsModule : RadarrRestModuleWithSignalR<QueueResource, NzbDrone.Core.Queue.Queue>,
                               IHandle<QueueUpdatedEvent>, IHandle<PendingReleasesUpdatedEvent>
    {
        private readonly IQueueService _queueService;
        private readonly IPendingReleaseService _pendingReleaseService;

        public QueueDetailsModule(IBroadcastSignalRMessage broadcastSignalRMessage, IQueueService queueService, IPendingReleaseService pendingReleaseService)
            : base(broadcastSignalRMessage, "queue/details")
        {
            _queueService = queueService;
            _pendingReleaseService = pendingReleaseService;
            GetResourceAll = GetQueue;
        }

        private List<QueueResource> GetQueue()
        {
            var includeMovie = Request.GetBooleanQueryParameter("includeMovie");
            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();
            var fullQueue = queue.Concat(pending);

            var movieIdQuery = Request.Query.MovieId;

            if (movieIdQuery.HasValue)
            {
                return fullQueue.Where(q => q.Movie.Id == (int)movieIdQuery).ToResource(includeMovie);
            }

            return fullQueue.ToResource(includeMovie);
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
