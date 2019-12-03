using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Validation;
using Radarr.Http;
using Radarr.Http.Mapping;

namespace Radarr.Api.V3.Profiles.Quality
{
    public class QualityProfileModule : RadarrRestModule<QualityProfileResource>
    {
        private readonly IProfileService _profileService;
        private readonly ICustomFormatService _formatService;

        public QualityProfileModule(IProfileService profileService, ICustomFormatService formatService)
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
                all.Add(CustomFormat.None.Id);
                var ids = items.Select(i => i.Format.Id);

                return all.Except(ids).Empty();
            }).WithMessage("All Custom Formats and no extra ones need to be present inside your Profile! Try refreshing your browser.");
            SharedValidator.RuleFor(c => c.FormatCutoff)
                .Must(c => _formatService.All().Select(f => f.Id).Contains(c) || c == CustomFormat.None.Id).WithMessage("The Custom Format Cutoff must be a valid Custom Format! Try refreshing your browser.");

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            CreateResource = Create;
            DeleteResource = DeleteProfile;
        }

        private int Create(QualityProfileResource resource)
        {
            var model = resource.ToModel();
            model = _profileService.Add(model);
            return model.Id;
        }

        private void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        private void Update(QualityProfileResource resource)
        {
            var model = resource.ToModel();

            _profileService.Update(model);
        }

        private QualityProfileResource GetById(int id)
        {
            return _profileService.Get(id).ToResource();
        }

        private List<QualityProfileResource> GetAll()
        {
            return _profileService.All().ToResource();
        }
    }
}
