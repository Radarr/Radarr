using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Radarr
{
    public class RadarrSettingsValidator : AbstractValidator<RadarrSettings>
    {
        public RadarrSettingsValidator()
        {
            RuleFor(c => c.BaseUrl).ValidRootUrl();
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class RadarrSettings : IProviderConfig
    {
        private static readonly RadarrSettingsValidator Validator = new RadarrSettingsValidator();

        public RadarrSettings()
        {
            BaseUrl = "";
            ApiKey = "";
        }

        [FieldDefinition(0, Label = "Full URL", HelpText = "URL, including port, of the Radarr V3 instance to import from")]
        public string BaseUrl { get; set; }

        [FieldDefinition(1, Label = "API Key", HelpText = "Apikey of the Radarr V3 instance to import from")]
        public string ApiKey { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
