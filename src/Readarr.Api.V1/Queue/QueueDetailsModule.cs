using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;
using NzbDrone.SignalR;
using Readarr.Http;
using Readarr.Http.Extensions;

namespace Readarr.Api.V1.Queue
{
    public class QueueDetailsModule : ReadarrRestModuleWithSignalR<QueueResource, NzbDrone.Core.Queue.Queue>,
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
            var includeAuthor = Request.GetBooleanQueryParameter("includeAuthor");
            var includeBook = Request.GetBooleanQueryParameter("includeBook", true);
            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();
            var fullQueue = queue.Concat(pending);

            var authorIdQuery = Request.Query.AuthorId;
            var bookIdsQuery = Request.Query.BookIds;

            if (authorIdQuery.HasValue)
            {
                return fullQueue.Where(q => q.Author?.Id == (int)authorIdQuery).ToResource(includeAuthor, includeBook);
            }

            if (bookIdsQuery.HasValue)
            {
                string bookIdsValue = bookIdsQuery.Value.ToString();

                var bookIds = bookIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(e => Convert.ToInt32(e))
                                                .ToList();

                return fullQueue.Where(q => q.Book != null && bookIds.Contains(q.Book.Id)).ToResource(includeAuthor, includeBook);
            }

            return fullQueue.ToResource(includeAuthor, includeBook);
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
