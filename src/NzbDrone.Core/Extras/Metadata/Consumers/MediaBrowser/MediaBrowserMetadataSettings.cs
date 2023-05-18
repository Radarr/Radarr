using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.MediaBrowser
{
    public class MediaBrowserSettingsValidator : AbstractValidator<MediaBrowserMetadataSettings>
    {
    }

    public class MediaBrowserMetadataSettings : IProviderConfig
    {
        private static readonly MediaBrowserSettingsValidator Validator = new MediaBrowserSettingsValidator();

        public MediaBrowserMetadataSettings()
        {
            MovieMetadata = true;
        }

        [FieldDefinition(0, Label = "Movie Metadata", Type = FieldType.Checkbox)]
        public bool MovieMetadata { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
