using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Download.Pending;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Queue;
using NzbDrone.SignalR;
using Lidarr.Http;
using Lidarr.Http.Extensions;

namespace Lidarr.Api.V1.Queue
{
    public class QueueDetailsModule : LidarrRestModuleWithSignalR<QueueResource, NzbDrone.Core.Queue.Queue>,
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
            var includeArtist = Request.GetBooleanQueryParameter("includeArtist");
            var includeAlbum = Request.GetBooleanQueryParameter("includeAlbum", true);
            var queue = _queueService.GetQueue();
            var pending = _pendingReleaseService.GetPendingQueue();
            var fullQueue = queue.Concat(pending);

            var artistIdQuery = Request.Query.ArtistId;
            var albumIdsQuery = Request.Query.AlbumIds;

            if (artistIdQuery.HasValue)
            {
                return fullQueue.Where(q => q.Artist?.Id == (int)artistIdQuery).ToResource(includeArtist, includeAlbum);
            }

            if (albumIdsQuery.HasValue)
            {
                string albumIdsValue = albumIdsQuery.Value.ToString();

                var albumIds = albumIdsValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(e => Convert.ToInt32(e))
                                                .ToList();

                return fullQueue.Where(q => q.Album != null && albumIds.Contains(q.Album.Id)).ToResource(includeArtist, includeAlbum);
            }

            return fullQueue.ToResource(includeArtist, includeAlbum);
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
