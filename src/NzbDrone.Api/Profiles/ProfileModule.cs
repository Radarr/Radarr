using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using NzbDrone.Common.Extensions;
using NzbDrone.Core.CustomFormats;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.Validation;
using Radarr.Http;
using Radarr.Http.Mapping;

namespace NzbDrone.Api.Profiles
{
    public class ProfileModule : RadarrRestModule<ProfileResource>
    {
        private readonly IProfileService _profileService;
        private readonly ICustomFormatService _formatService;

        public ProfileModule(IProfileService profileService, ICustomFormatService formatService)
        {
            _profileService = profileService;
            _formatService = formatService;
            SharedValidator.RuleFor(c => c.Name).NotEmpty();
            SharedValidator.RuleFor(c => c.Cutoff).NotNull();
            SharedValidator.RuleFor(c => c.Items).MustHaveAllowedQuality();
            SharedValidator.RuleFor(c => c.Language).ValidLanguage();
            SharedValidator.RuleFor(c => c.FormatItems).Must(items =>
            {
                var all = _formatService.All().Select(f => f.Id).ToList();
                all.Add(CustomFormat.None.Id);
                var ids = items.Select(i => i.Format.Id);

                return all.Except(ids).Empty();
            }).WithMessage("All Custom Formats and no extra ones need to be present inside your Profile! Try refreshing your browser.");
            SharedValidator.RuleFor(c => c.FormatCutoff)
                .Must(c => _formatService.All().Select(f => f.Id).Contains(c.Id) || c.Id == CustomFormat.None.Id).WithMessage("The Custom Format Cutoff must be a valid Custom Format! Try refreshing your browser.");

            GetResourceAll = GetAll;
            GetResourceById = GetById;
            UpdateResource = Update;
            CreateResource = Create;
            DeleteResource = DeleteProfile;
        }

        private int Create(ProfileResource resource)
        {
            var model = resource.ToModel();

            return _profileService.Add(model).Id;
        }

        private void DeleteProfile(int id)
        {
            _profileService.Delete(id);
        }

        private void Update(ProfileResource resource)
        {
            var model = resource.ToModel();

            _profileService.Update(model);
        }

        private ProfileResource GetById(int id)
        {
            return _profileService.Get(id).ToResource();
        }

        private List<ProfileResource> GetAll()
        {
            return _profileService.All().ToResource();
        }
    }
}
