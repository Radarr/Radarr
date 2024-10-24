using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Wdtv
{
    public class WdtvSettingsValidator : AbstractValidator<WdtvMetadataSettings>
    {
    }

    public class WdtvMetadataSettings : IProviderConfig
    {
        private static readonly WdtvSettingsValidator Validator = new WdtvSettingsValidator();

        public WdtvMetadataSettings()
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
