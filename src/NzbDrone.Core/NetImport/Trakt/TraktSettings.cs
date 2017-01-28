using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Profiles;
using NzbDrone.Core.ThingiProvider;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Trakt
{

    public class TraktSettings : NetImportBaseSettings
    {
        public TraktSettings()
        {
            Link = "https://api.trakt.tv/users/";
            Username = "";
            Listname = "";
        }

        [FieldDefinition(0, Label = "Trakt API URL", HelpText = "Link to to Trakt API URL, do not change unless you know what you are doing.")]
        public new string Link { get; set; }

        [FieldDefinition(1, Label = "Trakt Username", HelpText = "Trakt Username the list belongs to.")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "Trakt List Name", HelpText = "Trakt List Name")]
        public string Listname { get; set; }

    }

}
