using System;
using System.Collections.Generic;
using NzbDrone.Api.REST;
using NzbDrone.Core.Qualities;
using NzbDrone.Api.Music;
using NzbDrone.Core.Indexers;

namespace NzbDrone.Api.Blacklist
{
    public class BlacklistResource : RestResource
    {
        public int ArtistId { get; set; }
        public List<int> AlbumIds { get; set; }
        public string SourceTitle { get; set; }
        public QualityModel Quality { get; set; }
        public DateTime Date { get; set; }
        public DownloadProtocol Protocol { get; set; }
        public string Indexer { get; set; }
        public string Message { get; set; }

        public ArtistResource Artist { get; set; }
    }

    public static class BlacklistResourceMapper
    {
        public static BlacklistResource MapToResource(this Core.Blacklisting.Blacklist model)
        {
            if (model == null) return null;

            return new BlacklistResource
            {
                Id = model.Id,

                ArtistId = model.ArtistId,
                AlbumIds = model.AlbumIds,
                SourceTitle = model.SourceTitle,
                Quality = model.Quality,
                Date = model.Date,
                Protocol = model.Protocol,
                Indexer = model.Indexer,
                Message = model.Message,

                Artist = model.Artist.ToResource()
            };
        }
    }
}
