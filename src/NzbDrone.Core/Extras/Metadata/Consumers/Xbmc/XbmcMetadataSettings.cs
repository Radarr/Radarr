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
            MovieMetadataURL = false;
            MovieImages = true;
            UseMovieNfo = false;
        }

        [FieldDefinition(0, Label = "Movie Metadata", Type = FieldType.Checkbox)]
        public bool MovieMetadata { get; set; }

        [FieldDefinition(1, Label = "Movie Metadata URL", Type = FieldType.Checkbox, HelpText = "Radarr will write the tmdb/imdb url in the .nfo file", Advanced = true)]
        public bool MovieMetadataURL { get; set; }

        [FieldDefinition(2, Label = "Movie Images", Type = FieldType.Checkbox)]
        public bool MovieImages { get; set; }

        [FieldDefinition(3, Label = "Use Movie.nfo", Type = FieldType.Checkbox, HelpText = "Radarr will write metadata to movie.nfo instead of the default <movie-filename>.nfo")]
        public bool UseMovieNfo { get; set; }

        public bool IsValid => true;

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
