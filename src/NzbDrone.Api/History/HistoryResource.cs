using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Api.Movies;
using NzbDrone.Core.History;
using NzbDrone.Core.Qualities;


namespace NzbDrone.Api.History
{
    public class HistoryResource : RestResource
    {
        public int MovieId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public bool QualityCutoffNotMet { get; set; }
        public DateTime Date { get; set; }
        public string DownloadId { get; set; }

        public HistoryEventType EventType { get; set; }

        public Dictionary<string, string> Data { get; set; }
        public MovieResource Movie { get; set; }
    }

    public static class HistoryResourceMapper
    {
        public static HistoryResource ToResource(this Core.History.History model)
        {
            if (model == null) return null;

            return new HistoryResource
            {
                Id = model.Id,
                MovieId = model.MovieId,
                SourceTitle = model.SourceTitle,
                Quality = model.Quality,
                //QualityCutoffNotMet
                Date = model.Date,
                DownloadId = model.DownloadId,

                EventType = model.EventType,

                Data  = model.Data
            };
        }
    }
}
