using System;
using System.Collections.Generic;
using NzbDrone.Api.Movie;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Series;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.Blacklist
{
    public class BlacklistResource : RestResource
    {
        public int SeriesId { get; set; }
        public List<int> EpisodeIds { get; set; }
        public int MovieId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }
        public MovieResource Movie { get; set; }
        public SeriesResource Series { get; set; }
    }

    public static class BlacklistResourceMapper
    {
        public static BlacklistResource MapToResource(this Core.Blacklisting.Blacklist model)
        {
            if (model == null) return null;

            return new BlacklistResource
            {
                Id = model.Id,
                MovieId = model.MovieId,
                SeriesId = model.SeriesId,
                EpisodeIds = model.EpisodeIds,
                SourceTitle = model.SourceTitle,
                Quality = model.Quality,
                Date = model.Date,
                Protocol = model.Protocol,
                Indexer = model.Indexer,
                Message = model.Message,
                Movie = model.Movie.ToResource(),
                Series = model.Series.ToResource()
            };
        }
    }
}
