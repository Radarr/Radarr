using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Roksbox
{
    public class RoksboxSettingsValidator : AbstractValidator<RoksboxMetadataSettings>
    {
    }

    public class RoksboxMetadataSettings : IProviderConfig
    {
        private static readonly RoksboxSettingsValidator Validator = new RoksboxSettingsValidator();

        public RoksboxMetadataSettings()
        {
            MovieMetadata = true;
            MovieImages = true;
        }

        [FieldDefinition(0, Label = "MetadataSettingsMovieMetadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata)]
        public bool MovieMetadata { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsMovieImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image)]
        public bool MovieImages { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
