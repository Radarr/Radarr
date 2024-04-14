using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Trakt.User
{
    public class TraktUserSettingsValidator : TraktSettingsBaseValidator<TraktUserSettings>
    {
        public TraktUserSettingsValidator()
        {
            RuleFor(c => c.TraktListType).NotNull();
            RuleFor(c => c.AuthUser).NotEmpty();
        }
    }

    public class TraktUserSettings : TraktSettingsBase<TraktUserSettings>
    {
        private static readonly TraktUserSettingsValidator Validator = new ();

        public TraktUserSettings()
        {
            TraktListType = (int)TraktUserListType.UserWatchList;
        }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TraktUserListType), HelpText = "Type of list you're seeking to import from")]
        public int TraktListType { get; set; }

        [FieldDefinition(2, Label = "Username", HelpText = "Username for the List to import from (empty to use Auth User)")]
        public string Username { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
