using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.CouchPotato
{
    public class CouchPotatoSettingsValidator : AbstractValidator<CouchPotatoSettings>
    {
        public CouchPotatoSettingsValidator()
        {
            RuleFor(c => c.Link).ValidRootUrl();
            RuleFor(c => c.Port).InclusiveBetween(1, 65535);
            RuleFor(c => c.ApiKey).NotEmpty();
        }
    }

    public class CouchPotatoSettings : ImportListSettingsBase<CouchPotatoSettings>
    {
        private static readonly CouchPotatoSettingsValidator Validator = new ();

        public CouchPotatoSettings()
        {
            Link = "http://localhost";
            Port = 5050;
            UrlBase = "";
            OnlyActive = true;
        }

        [FieldDefinition(0, Label = "CouchPotato URL", HelpText = "URL to access your CouchPotato instance.")]
        public string Link { get; set; }

        [FieldDefinition(1, Label = "CouchPotato Port", HelpText = "Port your CouchPotato instance uses.")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "CouchPotato Url Base", HelpText = "If you have CouchPotato configured via reverse proxy put the base path here. e.g. couchpotato. Leave blank for no base URL.")]
        public string UrlBase { get; set; }

        [FieldDefinition(3, Label = "CouchPotato API Key", Privacy = PrivacyLevel.ApiKey, HelpText = "CouchPotato API Key. This can found within Settings > General")]
        public string ApiKey { get; set; }

        [FieldDefinition(4, Label = "Only Wanted", HelpText = "Only add wanted movies.", Type = FieldType.Checkbox)]
        public bool OnlyActive { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
