using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.CouchPotato
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

    public class CouchPotatoSettings : NetImportBaseSettings
    {
        public CouchPotatoSettings()
        {
            Link = "http://localhost";
            Port = 5050;
            UrlBase = "";
            OnlyActive = true;
        }

        [FieldDefinition(0, Label = "CouchPotato URL", HelpText = "Link to your CoouchPootato.")]
        public new string Link { get; set; }

        [FieldDefinition(1, Label = "CouchPotato Port", HelpText = "Port your CoouchPootato uses.")]
        public int Port { get; set; }

        [FieldDefinition(2, Label = "CouchPotato Url Base",
            HelpText = "UrlBase your CoouchPootato uses, leave blank for none")]
        public string UrlBase { get; set; }

        [FieldDefinition(3, Label = "CouchPotato API Key", HelpText = "CoouchPootato API Key.")]
        public string ApiKey { get; set; }

        [FieldDefinition(4, Label = "Only Wanted", HelpText = "Only add wanted movies.", Type = FieldType.Checkbox)]
        public bool OnlyActive { get; set; }

    }

}
