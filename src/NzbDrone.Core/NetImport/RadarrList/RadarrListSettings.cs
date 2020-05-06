using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.RadarrList
{
    public class RadarrSettingsValidator : AbstractValidator<RadarrListSettings>
    {
        public RadarrSettingsValidator()
        {
            RuleFor(c => c.APIURL).ValidRootUrl();
        }
    }

    public class RadarrListSettings : IProviderConfig
    {
        private static readonly RadarrSettingsValidator Validator = new RadarrSettingsValidator();

        public RadarrListSettings()
        {
            APIURL = "https://api.radarr.video/v2";
            Path = "";
        }

        [FieldDefinition(0, Label = "Radarr API URL", HelpText = "Link to to Radarr API URL. Use https://staging.api.radarr.video if you are on nightly.")]
        public string APIURL { get; set; }

        [FieldDefinition(1, Label = "Path to list", HelpText = "Path to the list proxied by the Radarr API. Check the wiki for available lists.")]
        public string Path { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
