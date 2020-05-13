using System;
using System.Collections.Generic;
using Nancy;
using NzbDrone.Core.Books;
using Readarr.Http;
using Readarr.Http.REST;

namespace Readarr.Api.V1.Series
{
    public class SeriesModule : ReadarrRestModule<SeriesResource>
    {
        protected readonly ISeriesService _seriesService;

        public SeriesModule(ISeriesService seriesService)
        {
            _seriesService = seriesService;

            GetResourceAll = GetSeries;
        }

        private List<SeriesResource> GetSeries()
        {
            var authorIdQuery = Request.Query.AuthorId;

            if (!authorIdQuery.HasValue)
            {
                throw new BadRequestException("authorId must be provided");
            }

            int authorId = Convert.ToInt32(authorIdQuery.Value);

            return _seriesService.GetByAuthorId(authorId).ToResource();
        }
    }
}
