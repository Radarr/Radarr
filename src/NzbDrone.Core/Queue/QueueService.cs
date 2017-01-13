﻿using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Common.Crypto;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.Tv;

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
            if (trackedDownload.RemoteEpisode != null && trackedDownload.RemoteEpisode.Episodes != null && trackedDownload.RemoteEpisode.Episodes.Any())
            {
                foreach (var episode in trackedDownload.RemoteEpisode.Episodes)
                {
                    yield return MapEpisode(trackedDownload, episode);
                }
            }
            else if (trackedDownload.RemoteMovie != null && trackedDownload.RemoteMovie.Movie != null)
            {
                yield return MapMovie(trackedDownload, trackedDownload.RemoteMovie.Movie);
            }
        }

        private Queue MapMovie(TrackedDownload trackedDownload, Movie movie)
        {
            var queue = new Queue
            {
                Id = HashConverter.GetHashInt31(string.Format("trackedDownload-{0}", trackedDownload.DownloadItem.DownloadId)),
                Series = null,
                Episode = null,
                Quality = trackedDownload.RemoteMovie.ParsedMovieInfo.Quality,
                Title = trackedDownload.DownloadItem.Title,
                Size = trackedDownload.DownloadItem.TotalSize,
                Sizeleft = trackedDownload.DownloadItem.RemainingSize,
                Timeleft = trackedDownload.DownloadItem.RemainingTime,
                Status = trackedDownload.DownloadItem.Status.ToString(),
                TrackedDownloadStatus = trackedDownload.Status.ToString(),
                StatusMessages = trackedDownload.StatusMessages.ToList(),
                RemoteEpisode = trackedDownload.RemoteEpisode,
                RemoteMovie = trackedDownload.RemoteMovie,
                DownloadId = trackedDownload.DownloadItem.DownloadId,
                Protocol = trackedDownload.Protocol,
                Movie = movie
            };

            if (queue.Timeleft.HasValue)
            {
                queue.EstimatedCompletionTime = DateTime.UtcNow.Add(queue.Timeleft.Value);
            }

            return queue;
        }

        private Queue MapEpisode(TrackedDownload trackedDownload, Episode episode)
        {
            var queue = new Queue
            {
                Id = HashConverter.GetHashInt31(string.Format("trackedDownload-{0}-ep{1}", trackedDownload.DownloadItem.DownloadId, episode.Id)),
                Series = trackedDownload.RemoteEpisode.Series,
                Episode = episode,
                Quality = trackedDownload.RemoteEpisode.ParsedEpisodeInfo.Quality,
                Title = trackedDownload.DownloadItem.Title,
                Size = trackedDownload.DownloadItem.TotalSize,
                Sizeleft = trackedDownload.DownloadItem.RemainingSize,
                Timeleft = trackedDownload.DownloadItem.RemainingTime,
                Status = trackedDownload.DownloadItem.Status.ToString(),
                TrackedDownloadStatus = trackedDownload.Status.ToString(),
                StatusMessages = trackedDownload.StatusMessages.ToList(),
                RemoteEpisode = trackedDownload.RemoteEpisode,
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
