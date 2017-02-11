using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Trakt
{

    public class TraktSettingsValidator : AbstractValidator<TraktSettings>
    {
        public TraktSettingsValidator()
        {
            RuleFor(c => c.Link).ValidRootUrl();
            RuleFor(c => c.Username).NotEmpty();
        }
    }

    public class TraktSettings : NetImportBaseSettings
    {
        public TraktSettings()
        {
            Link = "https://api.trakt.tv";
            Username = "";
            Listname = "";
        }

        [FieldDefinition(0, Label = "Trakt API URL", HelpText = "Link to to Trakt API URL, do not change unless you know what you are doing.")]
        public new string Link { get; set; }

        [FieldDefinition(1, Label = "Trakt List Type", Type = FieldType.Select, SelectOptions = typeof(TraktListType), HelpText = "Trakt list type, custom or watchlist")]
        public int ListType { get; set; }

        [FieldDefinition(2, Label = "Trakt Username", HelpText = "Required for User List")]
        public string Username { get; set; }

        [FieldDefinition(3, Label = "Trakt List Name", HelpText = "Required for Custom List")]
        public string Listname { get; set; }

    }



}
