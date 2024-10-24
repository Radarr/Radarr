using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Languages;
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
            MovieMetadata = true;
            UseMovieNfo = false;
            MovieMetadataLanguage = (int)Language.English;
            MovieMetadataURL = false;
            AddCollectionName = true;
            MovieImages = true;
        }

        [FieldDefinition(0, Label = "MetadataSettingsMovieMetadata", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsMovieMetadataHelpText")]
        public bool MovieMetadata { get; set; }

        [FieldDefinition(1, Label = "MetadataSettingsMovieMetadataNfo", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsMovieMetadataNfoHelpText")]
        public bool UseMovieNfo { get; set; }

        [FieldDefinition(2, Label = "MetadataSettingsMovieMetadataLanguage", Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsMovieMetadataLanguageHelpText")]
        public int MovieMetadataLanguage { get; set; }

        [FieldDefinition(3, Label = "MetadataSettingsMovieMetadataUrl", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsMovieMetadataUrlHelpText", Advanced = true)]
        public bool MovieMetadataURL { get; set; }

        [FieldDefinition(4, Label = "MetadataSettingsMovieMetadataCollectionName", Type = FieldType.Checkbox, Section = MetadataSectionType.Metadata, HelpText = "MetadataXmbcSettingsMovieMetadataCollectionNameHelpText", Advanced = true)]
        public bool AddCollectionName { get; set; }

        [FieldDefinition(5, Label = "MetadataSettingsMovieImages", Type = FieldType.Checkbox, Section = MetadataSectionType.Image, HelpText = "fanart.jpg, poster.jpg")]
        public bool MovieImages { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
