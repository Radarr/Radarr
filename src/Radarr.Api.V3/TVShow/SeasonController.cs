using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.TV;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.TVShow
{
    [V3ApiController]
    public class SeasonController : RestControllerWithSignalR<SeasonResource, Season>
    {
        private readonly ISeasonService _seasonService;

        public SeasonController(
            IBroadcastSignalRMessage signalRBroadcaster,
            ISeasonService seasonService)
            : base(signalRBroadcaster)
        {
            _seasonService = seasonService;
        }

        [HttpGet]
        public List<SeasonResource> GetSeasons([FromQuery] int? tvShowId)
        {
            if (!tvShowId.HasValue)
            {
                return new List<SeasonResource>();
            }

            var seasons = _seasonService.GetSeasonsByTVShowId(tvShowId.Value);
            return seasons.ToResource();
        }

        protected override SeasonResource GetResourceById(int id)
        {
            var season = _seasonService.GetSeason(id);
            return season.ToResource();
        }

        [RestPutById]
        [Produces("application/json")]
        public ActionResult<SeasonResource> UpdateSeason([FromBody] SeasonResource resource)
        {
            var season = resource.ToModel();
            _seasonService.UpdateSeason(season);
            BroadcastResourceChange(ModelAction.Updated, resource);
            return Accepted(resource.Id);
        }

        [HttpPut("monitor")]
        [Produces("application/json")]
        public IActionResult SetSeasonMonitored([FromBody] SeasonMonitorResource resource)
        {
            var season = _seasonService.GetSeason(resource.SeasonId);
            season.Monitored = resource.Monitored;
            _seasonService.UpdateSeason(season);

            return Accepted();
        }
    }

    public class SeasonMonitorResource
    {
        public int SeasonId { get; set; }
        public bool Monitored { get; set; }
    }
}
