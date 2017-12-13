using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Extras.Metadata.Consumers.Xbmc
{
    public class XbmcSettingsValidator : AbstractValidator<XbmcMetadataSettings>
    {
    }

    public class XbmcMetadataSettings : IProviderConfig
    {
        private static readonly XbmcSettingsValidator Validator = new XbmcSettingsValidator();

        public XbmcMetadataSettings()
        {
            ArtistMetadata = true;
            AlbumMetadata = true;
            TrackMetadata = true;
            ArtistImages = true;
            AlbumImages = true;
        }

        [FieldDefinition(0, Label = "Artist Metadata", Type = FieldType.Checkbox)]
        public bool ArtistMetadata { get; set; }

        [FieldDefinition(1, Label = "Album Metadata", Type = FieldType.Checkbox)]
        public bool AlbumMetadata { get; set; }

        [FieldDefinition(2, Label = "Track Metadata", Type = FieldType.Checkbox)]
        public bool TrackMetadata { get; set; }

        [FieldDefinition(3, Label = "Artist Images", Type = FieldType.Checkbox)]
        public bool ArtistImages { get; set; }

        [FieldDefinition(4, Label = "Album Images", Type = FieldType.Checkbox)]
        public bool AlbumImages { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
