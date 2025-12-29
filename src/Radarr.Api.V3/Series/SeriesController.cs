using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Series;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;
using SeriesModel = NzbDrone.Core.Series.Series;

namespace Radarr.Api.V3.Series
{
    [V3ApiController]
    public class SeriesController : RestControllerWithSignalR<SeriesResource, SeriesModel>
    {
        private readonly ISeriesService _seriesService;

        public SeriesController(IBroadcastSignalRMessage signalRBroadcaster,
                                ISeriesService seriesService)
            : base(signalRBroadcaster)
        {
            _seriesService = seriesService;

            PostValidator.RuleFor(s => s.Title).NotEmpty();
        }

        [HttpGet]
        public List<SeriesResource> GetSeries(int? authorId = null)
        {
            List<SeriesModel> seriesList;

            if (authorId.HasValue)
            {
                seriesList = _seriesService.FindByAuthorId(authorId.Value);
            }
            else
            {
                seriesList = _seriesService.GetAllSeries();
            }

            return seriesList.ToResource();
        }

        protected override SeriesResource GetResourceById(int id)
        {
            var series = _seriesService.GetSeries(id);
            return series?.ToResource();
        }

        [RestPostById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<SeriesResource> AddSeries([FromBody] SeriesResource seriesResource)
        {
            var series = _seriesService.AddSeries(seriesResource.ToModel());
            return Created(series.Id);
        }

        [RestPutById]
        [Consumes("application/json")]
        [Produces("application/json")]
        public ActionResult<SeriesResource> UpdateSeries([FromBody] SeriesResource seriesResource)
        {
            var series = _seriesService.GetSeries(seriesResource.Id);
            var updatedSeries = _seriesService.UpdateSeries(seriesResource.ToModel(series));
            var resource = updatedSeries.ToResource();

            BroadcastResourceChange(ModelAction.Updated, resource);

            return Ok(resource);
        }

        [RestDeleteById]
        public ActionResult DeleteSeries(int id)
        {
            _seriesService.DeleteSeries(id);
            return NoContent();
        }
    }
}
