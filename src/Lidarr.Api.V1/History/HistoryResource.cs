using System;
using System.Collections.Generic;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;
using Lidarr.Api.V1.Albums;
using Lidarr.Api.V1.Artist;
using Lidarr.Api.V1.Tracks;
using Lidarr.Http.REST;

namespace Lidarr.Api.V1.History
{
    public class HistoryResource : RestResource
    {
        public int AlbumId { get; set; }
        public int ArtistId { get; set; }
        public int TrackId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public bool QualityCutoffNotMet { get; set; }
        public DateTime Date { get; set; }
        public string DownloadId { get; set; }

        public HistoryEventType EventType { get; set; }

        public Dictionary<string, string> Data { get; set; }

        public AlbumResource Album { get; set; }
        public ArtistResource Artist { get; set; }
        public TrackResource Track { get; set; }
    }

    public static class HistoryResourceMapper
    {
        public static HistoryResource ToResource(this NzbDrone.Core.History.History model)
        {
            if (model == null) return null;

            return new HistoryResource
            {
                Id = model.Id,

                AlbumId = model.AlbumId,
                ArtistId = model.ArtistId,
                TrackId = model.TrackId,
                SourceTitle = model.SourceTitle,
                Quality = model.Quality,
                //QualityCutoffNotMet
                Date = model.Date,
                DownloadId = model.DownloadId,

                EventType = model.EventType,

                Data = model.Data
                //Episode
                //Series
            };
        }
    }
}
