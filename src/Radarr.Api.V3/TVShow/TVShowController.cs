using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Datastore.Events;
using NzbDrone.Core.Messaging.Events;
using NzbDrone.Core.TV;
using NzbDrone.Core.TV.Events;
using NzbDrone.Core.Validation;
using NzbDrone.Core.Validation.Paths;
using NzbDrone.SignalR;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.TVShow
{
    [V3ApiController]
    public class TVShowController : RestControllerWithSignalR<TVShowResource, NzbDrone.Core.TV.TVShow>,
                                     IHandle<TVShowAddedEvent>,
                                     IHandle<TVShowEditedEvent>,
                                     IHandle<TVShowDeletedEvent>,
                                     IHandle<TVShowsBulkEditedEvent>
    {
        private readonly ITVShowService _tvShowService;
        private readonly ISeasonService _seasonService;

        public TVShowController(
            IBroadcastSignalRMessage signalRBroadcaster,
            ITVShowService tvShowService,
            ISeasonService seasonService,
            QualityProfileExistsValidator qualityProfileExistsValidator)
            : base(signalRBroadcaster)
        {
            _tvShowService = tvShowService;
            _seasonService = seasonService;

            SharedValidator.RuleFor(s => s.QualityProfileId).ValidId();
            SharedValidator.RuleFor(s => s.QualityProfileId).SetValidator(qualityProfileExistsValidator);

            PostValidator.RuleFor(s => s.TvdbId).GreaterThan(0);
            PostValidator.RuleFor(s => s.Title).NotEmpty();
            PostValidator.RuleFor(s => s.Path).IsValidPath().When(s => s.Path.IsNotNullOrWhiteSpace());
            PostValidator.RuleFor(s => s.RootFolderPath).IsValidPath().When(s => s.RootFolderPath.IsNotNullOrWhiteSpace());

            PutValidator.RuleFor(s => s.Path).IsValidPath();
        }

        [HttpGet]
        public List<TVShowResource> GetAll()
        {
            var tvShows = _tvShowService.GetAllTVShows();
            return tvShows.Select(s => MapToResource(s)).ToList();
        }

        protected override TVShowResource GetResourceById(int id)
        {
            var tvShow = _tvShowService.GetTVShow(id);
            return MapToResource(tvShow);
        }

        [RestPostById]
        [Produces("application/json")]
        public ActionResult<TVShowResource> AddTVShow([FromBody] TVShowResource resource)
        {
            var model = resource.ToModel();
            var tvShow = _tvShowService.AddTVShow(model);
            return Created(tvShow.Id);
        }

        [RestPutById]
        [Produces("application/json")]
        public ActionResult<TVShowResource> UpdateTVShow([FromBody] TVShowResource resource)
        {
            var model = resource.ToModel();
            _tvShowService.UpdateTVShow(model);
            BroadcastResourceChange(ModelAction.Updated, resource);
            return Accepted(resource.Id);
        }

        [RestDeleteById]
        public void DeleteTVShow(int id, [FromQuery] bool deleteFiles = false)
        {
            _tvShowService.DeleteTVShow(id, deleteFiles);
        }

        private TVShowResource MapToResource(NzbDrone.Core.TV.TVShow tvShow)
        {
            var resource = tvShow.ToResource();
            var seasons = _seasonService.GetSeasonsByTVShowId(tvShow.Id);
            resource.Seasons = seasons.ToResource();
            return resource;
        }

        [NonAction]
        public void Handle(TVShowAddedEvent message)
        {
            BroadcastResourceChange(ModelAction.Created, MapToResource(message.TVShow));
        }

        [NonAction]
        public void Handle(TVShowEditedEvent message)
        {
            BroadcastResourceChange(ModelAction.Updated, MapToResource(message.TVShow));
        }

        [NonAction]
        public void Handle(TVShowDeletedEvent message)
        {
            BroadcastResourceChange(ModelAction.Deleted, message.TVShow.ToResource());
        }

        [NonAction]
        public void Handle(TVShowsBulkEditedEvent message)
        {
            foreach (var tvShow in message.TVShows)
            {
                BroadcastResourceChange(ModelAction.Updated, MapToResource(tvShow));
            }
        }
    }
}
