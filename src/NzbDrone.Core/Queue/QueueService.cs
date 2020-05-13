using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Books;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.History;
using NzbDrone.Core.Messaging.Events;
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
        private static List<Queue> _queue = new List<Queue>();
        private readonly IHistoryService _historyService;

        public QueueService(IEventAggregator eventAggregator,
                            IHistoryService historyService)
        {
            _eventAggregator = eventAggregator;
            _historyService = historyService;
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
            if (trackedDownload.RemoteBook?.Books != null && trackedDownload.RemoteBook.Books.Any())
            {
                foreach (var book in trackedDownload.RemoteBook.Books)
                {
                    yield return MapQueueItem(trackedDownload, book);
                }
            }
            else
            {
                yield return MapQueueItem(trackedDownload, null);
            }
        }

        private Queue MapQueueItem(TrackedDownload trackedDownload, Book book)
        {
            bool downloadForced = false;
            var history = _historyService.Find(trackedDownload.DownloadItem.DownloadId, HistoryEventType.Grabbed).FirstOrDefault();
            if (history != null && history.Data.ContainsKey("downloadForced"))
            {
                downloadForced = bool.Parse(history.Data["downloadForced"]);
            }

            var queue = new Queue
            {
                Author = trackedDownload.RemoteBook?.Author,
                Book = book,
                Quality = trackedDownload.RemoteBook?.ParsedBookInfo.Quality ?? new QualityModel(Quality.Unknown),
                Title = Parser.Parser.RemoveFileExtension(trackedDownload.DownloadItem.Title),
                Size = trackedDownload.DownloadItem.TotalSize,
                Sizeleft = trackedDownload.DownloadItem.RemainingSize,
                Timeleft = trackedDownload.DownloadItem.RemainingTime,
                Status = trackedDownload.DownloadItem.Status.ToString(),
                TrackedDownloadStatus = trackedDownload.Status,
                TrackedDownloadState = trackedDownload.State,
                StatusMessages = trackedDownload.StatusMessages.ToList(),
                ErrorMessage = trackedDownload.DownloadItem.Message,
                RemoteBook = trackedDownload.RemoteBook,
                DownloadId = trackedDownload.DownloadItem.DownloadId,
                Protocol = trackedDownload.Protocol,
                DownloadClient = trackedDownload.DownloadItem.DownloadClient,
                Indexer = trackedDownload.Indexer,
                OutputPath = trackedDownload.DownloadItem.OutputPath.ToString(),
                DownloadForced = downloadForced
            };

            if (book != null)
            {
                queue.Id = HashConverter.GetHashInt31(string.Format("trackedDownload-{0}-book{1}", trackedDownload.DownloadItem.DownloadId, book.Id));
            }
            else
            {
                queue.Id = HashConverter.GetHashInt31(string.Format("trackedDownload-{0}", trackedDownload.DownloadItem.DownloadId));
            }

            if (queue.Timeleft.HasValue)
            {
                queue.EstimatedCompletionTime = DateTime.UtcNow.Add(queue.Timeleft.Value);
            }

            return queue;
        }

        public void Handle(TrackedDownloadRefreshedEvent message)
        {
            _queue = message.TrackedDownloads.OrderBy(c => c.DownloadItem.RemainingTime).SelectMany(MapQueue)
                            .ToList();

            _eventAggregator.PublishEvent(new QueueUpdatedEvent());
        }
    }
}
