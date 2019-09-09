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
            TrackMetadata = true;
            ArtistImages = true;
            AlbumImages = true;
        }

        [FieldDefinition(0, Label = "Track Metadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "Album\\filename.xml")]
        public bool TrackMetadata { get; set; }

        [FieldDefinition(1, Label = "Artist Images", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Artist Title.jpg")]
        public bool ArtistImages { get; set; }

        [FieldDefinition(2, Label = "Album Images", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "Album Title.jpg")]
        public bool AlbumImages { get; set; }
        
        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
