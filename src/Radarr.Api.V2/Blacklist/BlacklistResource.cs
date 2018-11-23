using System;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Qualities;
using Radarr.Api.V2.Movies;
using Radarr.Http.REST;

namespace Radarr.Api.V2.Blacklist
{
    public class BlacklistResource : RestResource
    {
        public int MovieId { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }

        public MovieResource Movie { get; set; }
    }

    public static class BlacklistResourceMapper
    {
        public static BlacklistResource MapToResource(this NzbDrone.Core.Blacklisting.Blacklist model)
        {
            if (model == null) return null;

            return new BlacklistResource
            {
                Id = model.Id,

                MovieId = model.MovieId,
                SourceTitle = model.SourceTitle,
                Quality = model.Quality,
                Date = model.Date,
                Protocol = model.Protocol,
                Indexer = model.Indexer,
                Message = model.Message,

                Movie = model.Movie.ToResource()
            };
        }
    }
}
