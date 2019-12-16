using FluentValidation.Validators;
using NzbDrone.Core.Profiles.Metadata;

namespace NzbDrone.Core.Validation
{
    public class MetadataProfileExistsValidator : PropertyValidator
    {
        private readonly IMetadataProfileService _profileService;

        public MetadataProfileExistsValidator(IMetadataProfileService profileService)
            : base("Metadata profile does not exist")
        {
            _profileService = profileService;
        }

        protected override bool IsValid(PropertyValidatorContext context)
        {
            if (context.PropertyValue == null) return true;
            if ((int)context.PropertyValue == 0) return true;

            return _profileService.Exists((int)context.PropertyValue);
        }
    }
}
