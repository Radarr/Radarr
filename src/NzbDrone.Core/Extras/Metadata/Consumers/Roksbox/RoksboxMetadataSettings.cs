using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Roksbox
{
    public class RoksboxSettingsValidator : AbstractValidator<RoksboxMetadataSettings>
    {
        public RoksboxSettingsValidator()
        {
        }
    }

    public class RoksboxMetadataSettings : IProviderConfig
    {
        private static readonly RoksboxSettingsValidator Validator = new RoksboxSettingsValidator();

        public RoksboxMetadataSettings()
        {
            EpisodeMetadata = true;
            ArtistImages = true;
            AlbumImages = true;
            EpisodeImages = true;
        }

        [FieldDefinition(0, Label = "Episode Metadata", Type = FieldType.Checkbox, HelpText = "Season##\\filename.xml")]
        public bool EpisodeMetadata { get; set; }

        [FieldDefinition(1, Label = "Artist Images", Type = FieldType.Checkbox, HelpText = "Artist Title.jpg")]
        public bool ArtistImages { get; set; }

        [FieldDefinition(2, Label = "Album Images", Type = FieldType.Checkbox, HelpText = "Album Title.jpg")]
        public bool AlbumImages { get; set; }

        [FieldDefinition(3, Label = "Episode Images", Type = FieldType.Checkbox, HelpText = "Season##\\filename.jpg")]
        public bool EpisodeImages { get; set; }
        
        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
