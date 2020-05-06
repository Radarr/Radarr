using System;
using System.Collections.Generic;
using NzbDrone.Core.Indexers;
using NzbDrone.Core.Qualities;
using Readarr.Api.V1.Artist;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Blacklist
{
    public class BlacklistResource : RestResource
    {
        public int AuthorId { get; set; }
        public List<int> BookIds { get; set; }
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
        public static BlacklistResource MapToResource(this NzbDrone.Core.Blacklisting.Blacklist model)
        {
            if (model == null)
            {
                return null;
            }

            return new BlacklistResource
            {
                Id = model.Id,

                AuthorId = model.AuthorId,
                BookIds = model.BookIds,
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
