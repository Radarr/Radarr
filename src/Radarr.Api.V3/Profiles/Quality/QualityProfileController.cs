using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles;
using Radarr.Http;
using Radarr.Http.REST;
using Radarr.Http.REST.Attributes;

namespace Radarr.Api.V3.Profiles.Quality
{
    [V3ApiController]
    public class QualityProfileController : RestController<QualityProfileResource>
    {
        private readonly IProfileService _profileService;
        private readonly ICustomFormatService _formatService;

        public QualityProfileController(IProfileService profileService, ICustomFormatService formatService)
        {
            _profileService = profileService;
            _formatService = formatService;
            SharedValidator.RuleFor(c => c.Name).NotEmpty();

            // TODO: Need to validate the cutoff is allowed and the ID/quality ID exists
            // TODO: Need to validate the Items to ensure groups have names and at no item has no name, no items and no quality
            SharedValidator.RuleFor(c => c.Cutoff).ValidCutoff();
            SharedValidator.RuleFor(c => c.Items).ValidItems();
            SharedValidator.RuleFor(c => c.FormatItems).Must(items =>
            {
                var all = _formatService.All().Select(f => f.Id).ToList();
                var ids = items.Select(i => i.Format);

                return all.Except(ids).Empty();
            }).WithMessage("All Custom Formats and no extra ones need to be present inside your Profile! Try refreshing your browser.");
            SharedValidator.RuleFor(c => c).Custom((profile, context) =>
            {
                if (profile.FormatItems.Where(x => x.Score > 0).Sum(x => x.Score) < profile.MinFormatScore &&
                    profile.FormatItems.Max(x => x.Score) < profile.MinFormatScore)
                {
                    context.AddFailure("Minimum Custom Format Score can never be satisfied");
                }
            });
        }

        [RestPostById]
        public ActionResult<QualityProfileResource> Create(QualityProfileResource resource)
        {
            var model = resource.ToModel();
            model = _profileService.Add(model);
            return Created(model.Id);
        }

        [RestDeleteById]
        public void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        [RestPutById]
        public ActionResult<QualityProfileResource> Update(QualityProfileResource resource)
        {
            var model = resource.ToModel();

            _profileService.Update(model);

            return Accepted(model.Id);
        }

        protected override QualityProfileResource GetResourceById(int id)
        {
            return _profileService.Get(id).ToResource();
        }

        [HttpGet]
        public List<QualityProfileResource> GetAll()
        {
            return _profileService.All().ToResource();
        }
    }
}
