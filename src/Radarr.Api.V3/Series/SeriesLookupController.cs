using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Series;
using Radarr.Http;
using Radarr.Http.REST;

namespace Radarr.Api.V3.Series
{
    [V3ApiController("series/lookup")]
    public class SeriesLookupController : RestController<SeriesResource>
    {
        private readonly ISeriesService _seriesService;

        public SeriesLookupController(ISeriesService seriesService)
        {
            _seriesService = seriesService;
        }

        [NonAction]
        public override ActionResult<SeriesResource> GetResourceByIdWithErrorHandler(int id)
        {
            throw new NotImplementedException();
        }

        protected override SeriesResource GetResourceById(int id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("foreignid")]
        [Produces("application/json")]
        public ActionResult<SeriesResource> SearchByForeignId(string foreignId)
        {
            var series = _seriesService.FindByForeignId(foreignId);
            if (series == null)
            {
                return NotFound();
            }

            return series.ToResource();
        }

        [HttpGet("title")]
        [Produces("application/json")]
        public ActionResult<SeriesResource> SearchByTitle(string title)
        {
            var series = _seriesService.FindByTitle(title);
            if (series == null)
            {
                return NotFound();
            }

            return series.ToResource();
        }

        [HttpGet("author")]
        [Produces("application/json")]
        public IEnumerable<SeriesResource> SearchByAuthor(int authorId)
        {
            var seriesList = _seriesService.FindByAuthorId(authorId);
            return seriesList.ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public IEnumerable<SeriesResource> Search([FromQuery] string term)
        {
            var allSeries = _seriesService.GetAllSeries();
            var results = new List<SeriesResource>();

            foreach (var series in allSeries)
            {
                if (series.Title != null &&
                    series.Title.Contains(term, StringComparison.OrdinalIgnoreCase))
                {
                    results.Add(series.ToResource());
                }
            }

            return results;
        }
    }
}
