using System;
using System.Collections.Generic;
using NzbDrone.Core.History;
using NzbDrone.Core.Languages;
using NzbDrone.Core.Qualities;
using Radarr.Api.V3.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V3.History
{
    public class HistoryResource : RestResource
    {
        public int MovieId { get; set; }
        public string SourceTitle { get; set; }
        public List<Language> Languages { get; set; }
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
        public static HistoryResource ToResource(this NzbDrone.Core.History.History model)
        {
            if (model == null) return null;

            return new HistoryResource
            {
                Id = model.Id,
                
                MovieId = model.MovieId,
                SourceTitle = model.SourceTitle,
                Languages = model.Languages,
                Quality = model.Quality,
                //QualityCutoffNotMet
                Date = model.Date,
                DownloadId = model.DownloadId,

                EventType = model.EventType,

                Data = model.Data
            };
        }
    }
}
