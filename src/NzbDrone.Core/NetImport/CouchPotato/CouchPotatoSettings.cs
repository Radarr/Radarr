using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.CouchPotato
{

    public class CouchPotatoSettings : NetImportBaseSettings
    {
        public CouchPotatoSettings()
        {
            Link = "http://localhost";
        }

        [FieldDefinition(0, Label = "CouchPotato URL", HelpText = "Link to your CoouchPootato.")]
        public new string Link { get; set; }

        [FieldDefinition(1, Label = "CouchPotato Port", HelpText = "Port your CoouchPootato uses.")]
        public string Port { get; set; }

        [FieldDefinition(2, Label = "CouchPotato API Key", HelpText = "CoouchPootato API Key.")]
        public string ApiKey { get; set; }
    }
}
