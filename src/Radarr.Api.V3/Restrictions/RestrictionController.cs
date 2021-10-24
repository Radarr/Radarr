using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.Restrictions;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Restrictions
{
    [V3ApiController]
    public class RestrictionController : RestController<RestrictionResource>
    {
        private readonly IRestrictionService _restrictionService;

        public RestrictionController(IRestrictionService restrictionService)
        {
            _restrictionService = restrictionService;

            SharedValidator.RuleFor(d => d).Custom((restriction, context) =>
            {
                if (restriction.Ignored.IsNullOrWhiteSpace() && restriction.Required.IsNullOrWhiteSpace())
                {
                    context.AddFailure("Either 'Must contain' or 'Must not contain' is required");
                }
            });
        }

        protected override RestrictionResource GetResourceById(int id)
        {
            return _restrictionService.Get(id).ToResource();
        }

        [HttpGet]
        public List<RestrictionResource> GetAll()
        {
            return _restrictionService.All().ToResource();
        }

        [RestPostById]
        public ActionResult<RestrictionResource> Create(RestrictionResource resource)
        {
            return Created(_restrictionService.Add(resource.ToModel()).Id);
        }

        [RestPutById]
        public ActionResult<RestrictionResource> Update(RestrictionResource resource)
        {
            _restrictionService.Update(resource.ToModel());
            return Accepted(resource.Id);
        }

        [RestDeleteById]
        public void DeleteRestriction(int id)
        {
            _restrictionService.Delete(id);
        }
    }
}
