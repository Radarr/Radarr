using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.Notifications.Plex.Server
{
    public class PlexServerSettingsValidator : AbstractValidator<PlexServerSettings>
    {
        public PlexServerSettingsValidator()
        {
            RuleFor(c => c.Host).ValidHost();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.MapFrom).NotEmpty().Unless(c => !c.UseMapping);
            RuleFor(c => c.MapTo).NotEmpty().Unless(c => !c.UseMapping);
        }
    }

    public class PlexServerSettings : IProviderConfig
    {
        private static readonly PlexServerSettingsValidator Validator = new PlexServerSettingsValidator();

        public PlexServerSettings()
        {
            Port = 32400;
            UpdateLibrary = true;
            SignIn = "startOAuth";
        }

        [FieldDefinition(0, Label = "Host")]
        public string Host { get; set; }

        [FieldDefinition(1, Label = "Port")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "Use SSL", Type = FieldType.Checkbox, HelpText = "Connect to Plex over HTTPS instead of HTTP")]
        public bool UseSsl { get; set; }

        [FieldDefinition(3, Label = "Auth Token", Type = FieldType.Textbox, Privacy = PrivacyLevel.ApiKey, Advanced = true)]
        public string AuthToken { get; set; }

        [FieldDefinition(4, Label = "Authenticate with Plex.tv", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        [FieldDefinition(5, Label = "Update Library", Type = FieldType.Checkbox)]
        public bool UpdateLibrary { get; set; }

        [FieldDefinition(6, Label = "Use Mapping", Type = FieldType.Checkbox)]
        public bool UseMapping { get; set; }

        [FieldDefinition(7, Label = "Map From", Type = FieldType.Textbox)]
        public string MapFrom { get; set; }

        [FieldDefinition(8, Label = "Map To", Type = FieldType.Textbox)]
        public string MapTo { get; set; }

        public bool IsValid => !string.IsNullOrWhiteSpace(Host);

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
