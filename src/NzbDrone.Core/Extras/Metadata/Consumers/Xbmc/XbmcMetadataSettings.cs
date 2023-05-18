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
            MovieMetadataURL = false;
            MovieMetadataLanguage = (int)Language.English;
            MovieImages = true;
            UseMovieNfo = false;
            AddCollectionName = true;
        }

        [FieldDefinition(0, Label = "Movie Metadata", Type = FieldType.Checkbox)]
        public bool MovieMetadata { get; set; }

        [FieldDefinition(1, Label = "Movie Metadata URL", Type = FieldType.Checkbox, HelpText = "Radarr will write the tmdb/imdb url in the .nfo file", Advanced = true)]
        public bool MovieMetadataURL { get; set; }

        [FieldDefinition(2, Label = "Metadata Language", Type = FieldType.Select, SelectOptions = typeof(RealLanguageFieldConverter), HelpText = "Radarr will write metadata in the selected language if available")]
        public int MovieMetadataLanguage { get; set; }

        [FieldDefinition(3, Label = "Movie Images", Type = FieldType.Checkbox)]
        public bool MovieImages { get; set; }

        [FieldDefinition(4, Label = "Use Movie.nfo", Type = FieldType.Checkbox, HelpText = "Radarr will write metadata to movie.nfo instead of the default <movie-filename>.nfo")]
        public bool UseMovieNfo { get; set; }

        [FieldDefinition(5, Label = "Collection Name", Type = FieldType.Checkbox, HelpText = "Radarr will write the collection name to the .nfo file", Advanced = true)]
        public bool AddCollectionName { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
