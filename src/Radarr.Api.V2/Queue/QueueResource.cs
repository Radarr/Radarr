using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Qualities;
using Radarr.Api.V2.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V2.Queue
{
    public class QueueResource : RestResource
    {
        public int MovieId { get; set; }
        public MovieResource Movie { get; set; }
        public QualityModel Quality { get; set; }
        public decimal Size { get; set; }
        public string Title { get; set; }
        public decimal Sizeleft { get; set; }
        public TimeSpan? Timeleft { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public string Status { get; set; }
        public string TrackedDownloadStatus { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }
        public string ErrorMessage { get; set; }
        public string DownloadId { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string DownloadClient { get; set; }
        public string Indexer { get; set; }
    }

    public static class QueueResourceMapper
    {
        public static QueueResource ToResource(this NzbDrone.Core.Queue.Queue model, bool includeMovie)
        {
            if (model == null) return null;

            return new QueueResource
            {
                Id = model.Id,
                MovieId = model.Movie.Id,
                Movie = includeMovie ? model.Movie.ToResource() : null,
                Quality = model.Quality,
                Size = model.Size,
                Title = model.Title,
                Sizeleft = model.Sizeleft,
                Timeleft = model.Timeleft,
                EstimatedCompletionTime = model.EstimatedCompletionTime,
                Status = model.Status,
                TrackedDownloadStatus = model.TrackedDownloadStatus,
                StatusMessages = model.StatusMessages,
                ErrorMessage = model.ErrorMessage,
                DownloadId = model.DownloadId,
                Protocol = model.Protocol,
                DownloadClient = model.DownloadClient,
                Indexer = model.Indexer
            };
        }

        public static List<QueueResource> ToResource(this IEnumerable<NzbDrone.Core.Queue.Queue> models, bool includeMovie)
        {
            return models.Select((m) => ToResource(m, includeMovie)).ToList();
        }
    }
}
