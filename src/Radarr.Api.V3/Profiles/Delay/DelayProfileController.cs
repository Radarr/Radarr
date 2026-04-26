using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Core.Profiles.Delay;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;
using Radarr.Http.Validation;

namespace Radarr.Api.V3.Profiles.Delay
{
    [V3ApiController]
    public class DelayProfileController : RestController<DelayProfileResource>
    {
        private readonly IDelayProfileService _delayProfileService;

        public DelayProfileController(IDelayProfileService delayProfileService, DelayProfileTagInUseValidator tagInUseValidator)
        {
            _delayProfileService = delayProfileService;

            SharedValidator.RuleFor(d => d.Tags).NotEmpty().When(d => d.Id != 1);
            SharedValidator.RuleFor(d => d.Tags).EmptyCollection<DelayProfileResource, int>().When(d => d.Id == 1);
            SharedValidator.RuleFor(d => d.Tags).SetValidator(tagInUseValidator);
            SharedValidator.RuleFor(d => d.UsenetDelay).GreaterThanOrEqualTo(0);
            SharedValidator.RuleFor(d => d.TorrentDelay).GreaterThanOrEqualTo(0);

            SharedValidator.RuleFor(d => d).Custom((delayProfile, context) =>
            {
                if (!delayProfile.EnableUsenet && !delayProfile.EnableTorrent)
                {
                    context.AddFailure("Either Usenet or Torrent should be enabled");
                }
            });
        }

        [RestPostById]
        [Consumes("application/json")]
        public Results<Created<DelayProfileResource>, NotFound> Create([FromBody] DelayProfileResource resource)
        {
            var model = resource.ToModel();
            model = _delayProfileService.Add(model);

            return Created(model.Id);
        }

        [RestDeleteById]
        public Ok DeleteProfile(int id)
        {
            if (id == 1)
            {
                throw new MethodNotAllowedException("Cannot delete global delay profile");
            }

            _delayProfileService.Delete(id);

            return TypedResults.Ok();
        }

        [RestPutById]
        [Consumes("application/json")]
        public Results<Accepted<DelayProfileResource>, NotFound> Update([FromBody] DelayProfileResource resource)
        {
            var model = resource.ToModel();
            _delayProfileService.Update(model);
            return Accepted(model.Id);
        }

        protected override DelayProfileResource GetResourceById(int id)
        {
            return _delayProfileService.Get(id).ToResource();
        }

        [HttpGet]
        [Produces("application/json")]
        public Ok<List<DelayProfileResource>> GetAll()
        {
            return TypedResults.Ok(_delayProfileService.All().ToResource());
        }

        [HttpPut("reorder/{id:int}")]
        [Produces("application/json")]
        public Ok<List<DelayProfileResource>> Reorder([FromRoute] int id, [FromQuery] int? after)
        {
            ValidateId(id);

            return TypedResults.Ok(_delayProfileService.Reorder(id, after).ToResource());
        }
    }
}
