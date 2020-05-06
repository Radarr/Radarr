using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Goodreads
{
    public class GoodreadsSettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
        where TSettings : GoodreadsSettingsBase<TSettings>
    {
        public GoodreadsSettingsBaseValidator()
        {
            RuleFor(c => c.AccessToken).NotEmpty();
            RuleFor(c => c.AccessTokenSecret).NotEmpty();
        }
    }

    public class GoodreadsSettingsBase<TSettings> : IImportListSettings
        where TSettings : GoodreadsSettingsBase<TSettings>
    {
        public GoodreadsSettingsBase()
        {
            SignIn = "startOAuth";
        }

        public string BaseUrl { get; set; }

        public string ConsumerKey => "xQh8LhdTztb9u3cL26RqVg";
        public string ConsumerSecret => "96aDA1lJRcS8KofYbw2jjkRk3wTNKypHAL2GeOgbPZw";
        public string OAuthUrl => "https://www.goodreads.com/oauth/authorize";
        public string OAuthRequestTokenUrl => "https://www.goodreads.com/oauth/request_token";
        public string OAuthAccessTokenUrl => "https://www.goodreads.com/oauth/access_token";

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "Access Token Secret", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessTokenSecret { get; set; }

        [FieldDefinition(0, Label = "Request Token Secret", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RequestTokenSecret { get; set; }

        [FieldDefinition(0, Label = "User Id", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string UserId { get; set; }

        [FieldDefinition(0, Label = "User Name", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string UserName { get; set; }

        [FieldDefinition(99, Label = "Authenticate with Goodreads", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        protected virtual AbstractValidator<TSettings> Validator => new GoodreadsSettingsBaseValidator<TSettings>();

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
