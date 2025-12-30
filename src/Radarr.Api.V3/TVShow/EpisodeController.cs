using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.TV;
using NzbDrone.Core.TV.Events;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.TVShow
{
    [V3ApiController]
    public class EpisodeController : RestControllerWithSignalR<EpisodeResource, Episode>,
                                      IHandle<EpisodeAddedEvent>,
                                      IHandle<EpisodeEditedEvent>,
                                      IHandle<EpisodeDeletedEvent>,
                                      IHandle<EpisodesBulkEditedEvent>
    {
        private readonly IEpisodeService _episodeService;

        // Reserved for future use (including TV show in episode response)
#pragma warning disable S4487
        private readonly ITVShowService _tvShowService;
#pragma warning restore S4487

        public EpisodeController(
            IBroadcastSignalRMessage signalRBroadcaster,
            IEpisodeService episodeService,
            ITVShowService tvShowService)
            : base(signalRBroadcaster)
        {
            _episodeService = episodeService;
            _tvShowService = tvShowService;
        }

        [HttpGet]
        public List<EpisodeResource> GetEpisodes([FromQuery] int? tvShowId, [FromQuery] int? seasonNumber)
        {
            if (!tvShowId.HasValue)
            {
                return new List<EpisodeResource>();
            }

            var episodes = seasonNumber.HasValue
                ? _episodeService.GetEpisodesByTVShowIdAndSeasonNumber(tvShowId.Value, seasonNumber.Value)
                : _episodeService.GetEpisodesByTVShowId(tvShowId.Value);

            return episodes.ToResource();
        }

        protected override EpisodeResource GetResourceById(int id)
        {
            var episode = _episodeService.GetEpisode(id);
            return episode.ToResource();
        }

        [RestPutById]
        [Produces("application/json")]
        public ActionResult<EpisodeResource> UpdateEpisode([FromBody] EpisodeResource resource)
        {
            var model = resource.ToModel();
            _episodeService.UpdateEpisode(model);
            BroadcastResourceChange(ModelAction.Updated, resource);
            return Accepted(resource.Id);
        }

        [HttpPut("monitor")]
        [Produces("application/json")]
        public IActionResult SetEpisodeMonitored([FromBody] EpisodeMonitorResource resource)
        {
            var episodes = _episodeService.GetEpisodes(resource.EpisodeIds);
            foreach (var episode in episodes)
            {
                episode.Monitored = resource.Monitored;
            }

            _episodeService.UpdateEpisodes(episodes);

            return Accepted();
        }

        [NonAction]
        public void Handle(EpisodeAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Created, message.Episode.ToResource());
        }

        [NonAction]
        public void Handle(EpisodeEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, message.Episode.ToResource());
        }

        [NonAction]
        public void Handle(EpisodeDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.Episode.ToResource());
        }

        [NonAction]
        public void Handle(EpisodesBulkEditedEvent message)
        {
            foreach (var episode in message.Episodes)
            {
                BroadcastResourceChange(ModelAction.Updated, episode.ToResource());
            }
        }
    }

    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarLint", "S6968", Justification = "Follows existing resource patterns")]
    [global::System.Diagnostics.CodeAnalysis.SuppressMessage("SonarAnalyzer.CSharp", "S6964", Justification = "Follows existing resource patterns - value types validated by FluentValidation")]
    public class EpisodeMonitorResource
    {
        public List<int> EpisodeIds { get; set; }
        public bool Monitored { get; set; }
    }
}
