using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Trakt.List
{
    public class TraktListSettingsValidator : TraktSettingsBaseValidator<TraktListSettings>
    {
        public TraktListSettingsValidator()
        {
            RuleFor(c => c.Username).NotEmpty();
            RuleFor(c => c.Listname).NotEmpty();
        }
    }

    public class TraktListSettings : TraktSettingsBase<TraktListSettings>
    {
        private static readonly TraktListSettingsValidator Validator = new ();

        [FieldDefinition(1, Label = "Username", Privacy = PrivacyLevel.UserName, HelpText = "Username for the List to import from")]
        public string Username { get; set; }

        [FieldDefinition(2, Label = "List Name", HelpText = "List name for import, list must be public or you must have access to the list")]
        public string Listname { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
