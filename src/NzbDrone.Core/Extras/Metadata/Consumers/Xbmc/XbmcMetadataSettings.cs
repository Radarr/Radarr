using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public class XbmcSettingsValidator : AbstractValidator<XbmcMetadataSettings>
    {
        public XbmcSettingsValidator()
        {
        }
    }

    public class XbmcMetadataSettings : IProviderConfig
    {
        private static readonly XbmcSettingsValidator Validator = new XbmcSettingsValidator();

        public XbmcMetadataSettings()
        {
            MovieMetadata = true;
            MovieImages = true;
        }

        [FieldDefinition(0, Label = "Movie Metadata", Type = FieldType.Checkbox)]
        public bool MovieMetadata { get; set; }

        [FieldDefinition(1, Label = "Movie Images", Type = FieldType.Checkbox)]
        public bool MovieImages { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
