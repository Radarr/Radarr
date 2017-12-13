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
            ArtistMetadata = true;
        }

        [FieldDefinition(0, Label = "Artist Metadata", Type = FieldType.Checkbox, HelpText = "artist.xml")]
        public bool ArtistMetadata { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
