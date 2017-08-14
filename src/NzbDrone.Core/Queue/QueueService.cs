using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Music;

namespace NzbDrone.Core.Queue
{
    public interface IQueueService
    {
        List<Queue> GetQueue();
        Queue Find(int id);
    }

    public class QueueService : IQueueService, IHandle<TrackedDownloadRefreshedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private static List<Queue> _queue = new List<Queue>();

        public QueueService(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public List<Queue> GetQueue()
        {
            return _queue;
        }

        public Queue Find(int id)
        {
            return _queue.SingleOrDefault(q => q.Id == id);
        }

        public void Handle(TrackedDownloadRefreshedEvent message)
        {
            _queue = message.TrackedDownloads.OrderBy(c => c.DownloadItem.RemainingTime).SelectMany(MapQueue)
                .ToList();

            _eventAggregator.PublishEvent(new QueueUpdatedEvent());
        }

        private IEnumerable<Queue> MapQueue(TrackedDownload trackedDownload)
        {
            if (trackedDownload.RemoteAlbum.Albums != null && trackedDownload.RemoteAlbum.Albums.Any())
            {
                foreach (var album in trackedDownload.RemoteAlbum.Albums)
                {
                    yield return MapAlbum(trackedDownload, album);
                }
            }
            else
            {
                // FIXME: Present queue items with unknown series/episodes
            }
        }

        private Queue MapAlbum(TrackedDownload trackedDownload, Album album)
        {
            var queue = new Queue
            {
                Id = HashConverter.GetHashInt31(string.Format("trackedDownload-{0}-album{1}", trackedDownload.DownloadItem.DownloadId, album.Id)),
                Artist = trackedDownload.RemoteAlbum.Artist,
                Album = album,
                Quality = trackedDownload.RemoteAlbum.ParsedAlbumInfo.Quality,
                Title = trackedDownload.DownloadItem.Title,
                Size = trackedDownload.DownloadItem.TotalSize,
                Sizeleft = trackedDownload.DownloadItem.RemainingSize,
                Timeleft = trackedDownload.DownloadItem.RemainingTime,
                Status = trackedDownload.DownloadItem.Status.ToString(),
                TrackedDownloadStatus = trackedDownload.Status.ToString(),
                StatusMessages = trackedDownload.StatusMessages.ToList(),
                RemoteAlbum = trackedDownload.RemoteAlbum,
                DownloadId = trackedDownload.DownloadItem.DownloadId,
                Protocol = trackedDownload.Protocol
            };

            if (queue.Timeleft.HasValue)
            {
                queue.EstimatedCompletionTime = DateTime.UtcNow.Add(queue.Timeleft.Value);
            }

            return queue;
        }
    }
}
