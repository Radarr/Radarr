using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Movies;
using NzbDrone.Core.Qualities;

namespace NzbDrone.Core.Queue
{
    public interface IQueueService
    {
        List<Queue> GetQueue();
        Queue Find(int id);
        void Remove(int id);
    }

    public class QueueService : IQueueService, IHandle<TrackedDownloadRefreshedEvent>
    {
        private readonly IEventAggregator _eventAggregator;
        private static List<Queue> _queue = new ();

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

        public void Remove(int id)
        {
            _queue.Remove(Find(id));
        }

        private IEnumerable<Queue> MapQueue(TrackedDownload trackedDownload)
        {
            if (trackedDownload.RemoteMovie != null && trackedDownload.RemoteMovie.Movie != null)
            {
                yield return MapMovie(trackedDownload, trackedDownload.RemoteMovie.Movie);
            }
            else
            {
                yield return MapMovie(trackedDownload, null);
            }
        }

        private Queue MapMovie(TrackedDownload trackedDownload, Movie movie)
        {
            var queue = new Queue
            {
                Languages = trackedDownload.RemoteMovie?.Languages ?? new List<Language> { Language.Unknown },
                Quality = trackedDownload.RemoteMovie?.ParsedMovieInfo.Quality ?? new QualityModel(Quality.Unknown),
                Title = trackedDownload.DownloadItem.Title,
                Size = trackedDownload.DownloadItem.TotalSize,
                SizeLeft = trackedDownload.DownloadItem.RemainingSize,
                TimeLeft = trackedDownload.DownloadItem.RemainingTime,
                Status = Enum.TryParse(trackedDownload.DownloadItem.Status.ToString(), out QueueStatus outValue) ? outValue : QueueStatus.Unknown,
                TrackedDownloadStatus = trackedDownload.Status,
                TrackedDownloadState = trackedDownload.State,
                StatusMessages = trackedDownload.StatusMessages.ToList(),
                ErrorMessage = trackedDownload.DownloadItem.Message,
                RemoteMovie = trackedDownload.RemoteMovie,
                DownloadId = trackedDownload.DownloadItem.DownloadId,
                Protocol = trackedDownload.Protocol,
                Movie = movie,
                DownloadClient = trackedDownload.DownloadItem.DownloadClientInfo.Name,
                Indexer = trackedDownload.Indexer,
                OutputPath = trackedDownload.DownloadItem.OutputPath.ToString(),
                Added = trackedDownload.Added,
                DownloadClientHasPostImportCategory = trackedDownload.DownloadItem.DownloadClientInfo.HasPostImportCategory
            };

            queue.Id = HashConverter.GetHashInt31($"trackedDownload-{trackedDownload.DownloadClient}-{trackedDownload.DownloadItem.DownloadId}");

            if (queue.TimeLeft.HasValue)
            {
                queue.EstimatedCompletionTime = DateTime.UtcNow.Add(queue.TimeLeft.Value);
            }

            return queue;
        }

        public void Handle(TrackedDownloadRefreshedEvent message)
        {
            _queue = message.TrackedDownloads
                .Where(t => t.IsTrackable)
                .OrderBy(c => c.DownloadItem.RemainingTime)
                .SelectMany(MapQueue)
                .ToList();

            _eventAggregator.PublishEvent(new QueueUpdatedEvent());
        }
    }
}
