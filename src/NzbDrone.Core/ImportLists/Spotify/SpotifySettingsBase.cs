using System;
using FluentValidation;
using NzbDrone.Core.Annotations;
using NzbDrone.Core.Validation;

namespace NzbDrone.Core.ImportLists.Spotify
{
    public class SpotifySettingsBaseValidator<TSettings> : AbstractValidator<TSettings>
        where TSettings : SpotifySettingsBase<TSettings>
    {
        public SpotifySettingsBaseValidator()
        {
            RuleFor(c => c.AccessToken).NotEmpty();
            RuleFor(c => c.RefreshToken).NotEmpty();
            RuleFor(c => c.Expires).NotEmpty();
        }
    }

    public class SpotifySettingsBase<TSettings> : IImportListSettings
        where TSettings : SpotifySettingsBase<TSettings>
    {
        protected virtual AbstractValidator<TSettings> Validator => new SpotifySettingsBaseValidator<TSettings>();

        public SpotifySettingsBase()
        {
            BaseUrl = "https://api.spotify.com/v1";
            SignIn = "startOAuth";
        }

        public string BaseUrl { get; set; }

        public string OAuthUrl => "https://accounts.spotify.com/authorize";
        public string RedirectUri => "https://spotify.lidarr.audio/auth";
        public string RenewUri => "https://spotify.lidarr.audio/renew";
        public string ClientId => "848082790c32436d8a0405fddca0aa18";
        public virtual string Scope => "";

        [FieldDefinition(0, Label = "Access Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string AccessToken { get; set; }

        [FieldDefinition(0, Label = "Refresh Token", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public string RefreshToken { get; set; }

        [FieldDefinition(0, Label = "Expires", Type = FieldType.Textbox, Hidden = HiddenType.Hidden)]
        public DateTime Expires { get; set; }

        [FieldDefinition(99, Label = "Authenticate with Spotify", Type = FieldType.OAuth)]
        public string SignIn { get; set; }

        public NzbDroneValidationResult Validate()
        {
            return new NzbDroneValidationResult(Validator.Validate((TSettings)this));
        }
    }
}
