using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Kometa
{
    public class KometaSettingsValidator : AbstractValidator<KometaMetadataSettings>
    {
    }

    public class KometaMetadataSettings : IProviderConfig
    {
        private static readonly KometaSettingsValidator Validator = new ();

        public KometaMetadataSettings()
        {
            MovieImages = true;
        }

        [FieldDefinition(0, Label = "MetadataSettingsMovieImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "poster.jpg, background.jpg")]
        public bool MovieImages { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
