using System;
using System.Collections.Generic;
using System.Linq;
using NzbDrone.Core.Download.TrackedDownloads;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using NzbDrone.Core.Queue;
using Radarr.Api.V3.CustomFormats;
using Radarr.Api.V3.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Queue
{
    public class QueueResource : RestResource
    {
        public int? MovieId { get; set; }
        public MovieResource Movie { get; set; }
        public List<Language> Languages { get; set; }
        public QualityModel Quality { get; set; }
        public List<CustomFormatResource> CustomFormats { get; set; }
        public int CustomFormatScore { get; set; }
        public decimal Size { get; set; }
        public string Title { get; set; }

        // Collides with existing properties due to case-insensitive deserialization
        // public decimal SizeLeft { get; set; }
        // public TimeSpan? TimeLeft { get; set; }

        public DateTime? EstimatedCompletionTime { get; set; }
        public DateTime? Added { get; set; }
        public QueueStatus Status { get; set; }
        public TrackedDownloadStatus? TrackedDownloadStatus { get; set; }
        public TrackedDownloadState? TrackedDownloadState { get; set; }
        public List<TrackedDownloadStatusMessage> StatusMessages { get; set; }
        public string ErrorMessage { get; set; }
        public string DownloadId { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string DownloadClient { get; set; }
        public bool DownloadClientHasPostImportCategory { get; set; }
        public string Indexer { get; set; }
        public string OutputPath { get; set; }

        [Obsolete("Will be replaced by SizeLeft")]
        public decimal Sizeleft { get; set; }

        [Obsolete("Will be replaced by TimeLeft")]
        public TimeSpan? Timeleft { get; set; }
    }

    public static class QueueResourceMapper
    {
        public static QueueResource ToResource(this NzbDrone.Core.Queue.Queue model, bool includeMovie)
        {
            if (model == null)
            {
                return null;
            }

            var customFormats = model.RemoteMovie?.CustomFormats;
            var customFormatScore = model.Movie?.QualityProfile?.CalculateCustomFormatScore(customFormats) ?? 0;

            return new QueueResource
            {
                Id = model.Id,
                MovieId = model.Movie?.Id,
                Movie = includeMovie && model.Movie != null ? model.Movie.ToResource(0) : null,
                Languages = model.Languages,
                Quality = model.Quality,
                CustomFormats = customFormats?.ToResource(false),
                CustomFormatScore = customFormatScore,
                Size = model.Size,
                Title = model.Title,

                // Collides with existing properties due to case-insensitive deserialization
                // SizeLeft = model.SizeLeft,
                // TimeLeft = model.TimeLeft,

                EstimatedCompletionTime = model.EstimatedCompletionTime,
                Added = model.Added,
                Status = model.Status,
                TrackedDownloadStatus = model.TrackedDownloadStatus,
                TrackedDownloadState = model.TrackedDownloadState,
                StatusMessages = model.StatusMessages,
                ErrorMessage = model.ErrorMessage,
                DownloadId = model.DownloadId,
                Protocol = model.Protocol,
                DownloadClient = model.DownloadClient,
                DownloadClientHasPostImportCategory = model.DownloadClientHasPostImportCategory,
                Indexer = model.Indexer,
                OutputPath = model.OutputPath,

                #pragma warning disable CS0618
                Sizeleft = model.SizeLeft,
                Timeleft = model.TimeLeft,
                #pragma warning restore CS0618
            };
        }

        public static List<QueueResource> ToResource(this IEnumerable<NzbDrone.Core.Queue.Queue> models, bool includeMovie)
        {
            return models.Select((m) => ToResource(m, includeMovie)).ToList();
        }
    }
}
