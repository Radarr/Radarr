using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.NetImport.Trakt
{

    public class TraktSettingsValidator : AbstractValidator<TraktSettings>
    {
        public TraktSettingsValidator()
        {

        }
    }

    public class TraktSettings : NetImportBaseSettings
    {
        private static readonly TraktSettingsValidator Validator = new TraktSettingsValidator();

        public TraktSettings()
        {
            Username = "";
            Listname = "";
        }

        [FieldDefinition(0, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TraktListType), HelpText = "Trakt list type")]
        public int ListType { get; set; }

        [FieldDefinition(1, Label = "Username", HelpText = "Required for User List")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "List Name", HelpText = "Required for Custom List")]
        public string Listname { get; set; }


        public new NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }

}
