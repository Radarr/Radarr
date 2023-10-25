using FluentValidation.Validators;
using NzbDrone.Core.Profiles.Qualities;

namespace NzbDrone.Core.Validation
{
    public class ProfileExistsValidator : PropertyValidator
    {
        private readonly IQualityProfileService _profileService;

        public ProfileExistsValidator(IQualityProfileService profileService)
        {
            _profileService = profileService;
        }

        protected override string GetDefaultMessageTemplate() => "QualityProfile does not exist";

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context?.PropertyValue == null || (int)context.PropertyValue == 0)
            {
                return true;
            }

            return _profileService.Exists((int)context.PropertyValue);
        }
    }
}
