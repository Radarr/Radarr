using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.TMDb.User
{
    public class TMDbUserSettingsValidator : TMDbSettingsBaseValidator<TMDbUserSettings>
    {
        public TMDbUserSettingsValidator()
        {
            RuleFor(c => c.ListType).NotEmpty();
            RuleFor(c => c.AccessToken).NotEmpty();
            RuleFor(c => c.AccountId).NotEmpty();
        }
    }

    public class TMDbUserSettings : TMDbSettingsBase<TMDbUserSettings>
    {
        private static readonly TMDbUserSettingsValidator Validator = new ();

        public TMDbUserSettings()
        {
            ListType = (int)TMDbUserListType.Watchlist;
        }

        public string OAuthUrl => "https://www.themoviedb.org/auth/access";

        [FieldDefinition(0, Label = "Account Id", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccountId { get; set; }

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(1, Label = "List Type", Type = FieldType.Select, SelectOptions = typeof(TMDbUserListType), HelpText = "Type of list your seeking to import from")]
        public int ListType { get; set; }

        [FieldDefinition(99, Label = "Authenticate with TMDB", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public override NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate(this));
        }
    }
}
